using System;
using System.Reflection;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using Avalonia.Styling;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.DependencyInjection;
using IconPacks.Avalonia.RemixIcon;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using TombLauncher.Configuration;
using TombLauncher.Configuration.Sections;
using TombLauncher.Contracts.Localization;
using TombLauncher.Core.Exceptions;
using TombLauncher.Core.PlatformSpecific;
using TombLauncher.Data.Database;
using TombLauncher.Data.Database.Services;
using TombLauncher.Extensions;
using TombLauncher.Services;
using TombLauncher.Utils;
using TombLauncher.ViewModels;
using TombLauncher.Views;

namespace TombLauncher;

public class App : Application
{
    public App()
    {
        Name = Assembly.GetExecutingAssembly().GetName().Name;
    }

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override async void OnFrameworkInitializationCompleted()
    {
        try
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                // Line below is needed to remove Avalonia data validation.
                // Without this line you will get duplicate validations from both Avalonia and CT
                BindingPlugins.DataValidators.RemoveAt(0);
                var splashScreen = new SplashScreen();
                splashScreen.Show();
                desktop.ShutdownRequested += (_, _) =>
                {
                    Log.Logger.Information("Application is shutting down");
                    ((IDisposable)Log.Logger).Dispose();
                };

                Dispatcher.UIThread.UnhandledException += OnUnhandledException;

                // Run initialization in background to allow Splash Screen to render
                await Task.Run(async () =>
                {
                    await InitializeServices();
                    // Trigger DB migration explicitly in background
                    using var scope = Ioc.Default.CreateScope();
                    var dbContext = scope.ServiceProvider.GetRequiredService<TombLauncherDbContext>();
                    await dbContext.Database.MigrateAsync();
                });

                await Dispatcher.UIThread.InvokeAsync(async () => await ShowMainWindow(desktop, splashScreen));
            }

            base.OnFrameworkInitializationCompleted();
        }
        catch (Exception e)
        {
            Log.Logger.Fatal(e, "Unhandled exception occurred before OnFrameworkInitializationCompleted");
            Environment.Exit(-1);
        }
    }

    private void OnUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        AppCrashDataService? appCrashDataService = null;
        try
        {
            appCrashDataService = Ioc.Default.GetRequiredService<AppCrashDataService>();
        }
        catch (InvalidOperationException)
        {
            // Service provider not configured yet.
            // Log to console/debug as fallback
            Console.Error.WriteLine("Unhandled exception occurred before IoC container was initialized.");
            Console.Error.WriteLine(e.Exception);
            // Cannot use database logging
        }

        var exception = e.Exception;
        if (exception is TargetInvocationException tie)
        {
            exception = tie.InnerException;
        }

        Console.Error.WriteLine("--- ORIGINAL FATAL CRASH ---");
        Console.Error.WriteLine(exception);
        Console.Error.WriteLine("----------------------------");

        if (appCrashDataService != null)
        {
            if (exception != null) appCrashDataService.InsertAppCrash(exception);
            var welcomePageService = Ioc.Default.GetRequiredService<WelcomePageService>();
            welcomePageService.HandleNotNotifiedCrashes();
        }

        e.Handled = true;
        if (exception?.GetType() == typeof(AppRestartRequestedException))
        {
            // WTF?! How did you get in here?
            e.Handled = false;
        }
    }

    private async Task ShowMainWindow(IClassicDesktopStyleApplicationLifetime desktop, SplashScreen splashScreen)
    {
        // Services initialized in background task
        ApplyInitialSettings();
        var navigationManager = Ioc.Default.GetRequiredService<NavigationManager>();
        var mainWindow = new MainWindow
        {
            DataContext = new MainWindowViewModel(navigationManager,
                Ioc.Default.GetRequiredService<NotificationListViewModel>(),
                Ioc.Default.GetRequiredService<ISettingsProvider>(), Ioc.Default.GetRequiredService<IPlatformSpecificFeatures>()),
        };

        desktop.MainWindow = mainWindow;
        desktop.ShutdownMode = Avalonia.Controls.ShutdownMode.OnMainWindowClose;
        mainWindow.Show();
        splashScreen.Close();
        var updateService = Ioc.Default.GetRequiredService<UpdateService>();
        await updateService.StartAsync();
        await CheckCompatibilityToolAsync();
    }

    private async Task InitializeServices()
    {
        var platformSpecificFeatures = AppUtils.InitPlatformSpecificFeatures();

        var appDataDirectory = platformSpecificFeatures.GetAppDataDirectory();

        var serviceCollection = new ServiceCollection();
        serviceCollection.AddAppConfiguration(appDataDirectory)
            .AddPlatformSpecificFeatures(platformSpecificFeatures)
            .AddTombLauncherLogging(appDataDirectory)
            .AddTombLauncherMappings()
            .AddSaveGameManagement()
            .AddTheming()
            .AddFileOperations()
            .AddPageServices()
            .AddViewModels()
            .AddLocalization()
            .AddDatabaseAccess(appDataDirectory)
            .AddPopups()
            .AddNavigation()
            .AddGameManagement()
            .AddDownloaders()
            .AddNotifications()
            .AddUpdater();

        var serviceProvider = serviceCollection.BuildServiceProvider();
        Ioc.Default.ConfigureServices(serviceProvider);
        Log.Logger.Information("Service initialization complete");

        await Task.CompletedTask;
    }

    private void ApplyInitialSettings()
    {
        var settingsProvider = Ioc.Default.GetRequiredService<ISettingsProvider>();
        var localizationManager = Ioc.Default.GetRequiredService<ILocalizationManager>();
        var themeManager = Ioc.Default.GetRequiredService<ThemeManager>();

        var applicationLanguage = settingsProvider.GetApplicationSettings().ApplicationLanguage;
        localizationManager.ChangeLanguage(applicationLanguage);

        var applicationTheme = settingsProvider.GetAppearanceSettings().ApplicationTheme;
        themeManager.ApplyTheme(applicationTheme);

        var baseVariant = ThemeVariant.Dark;
        if (!string.IsNullOrEmpty(applicationTheme) && applicationTheme.Contains("Light"))
        {
            baseVariant = ThemeVariant.Light;
        }
        AppUtils.ChangeTheme(baseVariant);
    }

    private static async Task CheckCompatibilityToolAsync()
    {
        var platform = Ioc.Default.GetRequiredService<IPlatformSpecificFeatures>();
        if (!platform.IsWineSupported) return;

        var appConfig = Ioc.Default.GetRequiredService<ILayeredAppConfiguration>();
        var notifications = Ioc.Default.GetRequiredService<NotificationService>();

        var tool = appConfig.Compatibility.CompatibilityTool;

        if (tool == CompatibilityTool.Proton)
        {
            var protonInstallations = platform.FindAvailableProtonInstallations();
            if (protonInstallations.Count == 0 && string.IsNullOrWhiteSpace(appConfig.Compatibility.ProtonPath))
            {
                await notifications.AddWarningNotificationAsync(
                    "PROTON_NOT_FOUND", "PROTON_NOT_FOUND_DESCRIPTION",
                    PackIconRemixIconKind.GobletBrokenLine);
            }
        }
        else
        {
            // Wine (default)
            var mergedWinePath = appConfig.Compatibility.WinePath;
            var wineExe = platform.FindWineExecutable();
            if (wineExe != null)
            {
                if (mergedWinePath != wineExe)
                {
                    appConfig.User.Compatibility.WinePath = wineExe;
                    await PersistAsync();
                }
            }
            else
            {
                await notifications.AddWarningNotificationAsync(
                    "WINE_NOT_FOUND", "WINE_NOT_FOUND_DESCRIPTION",
                    PackIconRemixIconKind.GobletBrokenLine);
            }
        }

        static async Task PersistAsync()
        {
            using var scope = Ioc.Default.CreateScope();
            var settingsService = scope.ServiceProvider.GetRequiredService<SettingsPageService>();
            await settingsService.PersistCurrentConfigAsync();
        }
    }
}