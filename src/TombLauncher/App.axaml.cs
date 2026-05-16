using System;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
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
using TombLauncher.Ai.Extensions;
using TombLauncher.Ai.Models;
using TombLauncher.Ai.Services;
using TombLauncher.Configuration;
using TombLauncher.Contracts.Enums;
using TombLauncher.Contracts.Localization;
using TombLauncher.Core.Exceptions;
using TombLauncher.Core.Extensions;
using TombLauncher.Core.PlatformSpecific;
using TombLauncher.Core.Utils;
using TombLauncher.Data.Database;
using TombLauncher.Data.Database.Services;
using TombLauncher.Extensions;
using TombLauncher.Contracts.Integrations;
using TombLauncher.Integrations.Extensions;
using TombLauncher.Localization.Extensions;
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
                var splashScreen = new SplashScreen()
                {
                    Version = VersionUtils.GetApplicationVersion()
                };

                IProgress<string> progress = new Progress<string>(p => splashScreen.StatusMessage = p);
                splashScreen.Show();
                desktop.ShutdownRequested += (_, _) =>
                {
                    Log.Logger.Information("Application is shutting down");
                    ((IDisposable)Log.Logger).Dispose();
                };

                Dispatcher.UIThread.UnhandledException += OnUnhandledException;
                var resourceDictionary = new ResourceDictionary();

                var kbUpdateResult = KbUpdateResult.Success();

                try
                {
                    // Run initialization in background to allow Splash Screen to render
                    var mainWindowVm = await Task.Run(async () =>
                    {
                        await InitializeServices(Current!, resourceDictionary, progress);

                        progress.Report("Restoring language settings...");
                        var settingsProvider = Ioc.Default.GetRequiredService<ISettingsProvider>();
                        Dispatcher.UIThread.Invoke(() => { ApplyUiLanguage(settingsProvider); });

                        await MigrateWindowsLaunchConfig();

                        progress.Report("Updating application database...");
                        using var scope = Ioc.Default.CreateScope();
                        var dbContext = scope.ServiceProvider.GetRequiredService<TombLauncherDbContext>();
                        await dbContext.Database.MigrateAsync();

                        if (settingsProvider.GetAiCoreSettings().IsEnabled)
                        {
                            progress.Report("Updating KB database...");
                            kbUpdateResult = await Ioc.Default.GetRequiredService<KbUpdateService>()
                                .CheckAndUpdateAsync(progress, ct: default);
                        }
                        
                        progress.Report("Getting ready...");

                        // Resolved on a background thread: MainWindowViewModel and its dependencies
                        // must stay VM-pure (no AvaloniaObject/IImage/UI-thread-affine construction).
                        return Ioc.Default.GetRequiredService<MainWindowViewModel>();
                    });

                    // Built after ApplyUiLanguage so that {loc:Translate ...} markup extensions
                    // in MainWindow.axaml resolve against the loaded LocalizationManager.
                    var mainWindow = new MainWindow { DataContext = mainWindowVm };
                    desktop.MainWindow = mainWindow;
                    desktop.ShutdownMode = ShutdownMode.OnMainWindowClose;

                    await Dispatcher.UIThread.InvokeAsync(async () => await ShowMainWindow(splashScreen, mainWindow, kbUpdateResult));
                }
                catch (Exception ex)
                {
                    Log.Logger.Fatal(ex, "Fatal exception on background initialization task");
                    Environment.Exit(-1);
                }
            }

            base.OnFrameworkInitializationCompleted();
        }
        catch (Exception e)
        {
            Log.Logger.Fatal(e, "Unhandled exception occurred before OnFrameworkInitializationCompleted");
            Environment.Exit(-1);
        }
    }

    private static async Task MigrateWindowsLaunchConfig()
    {
        var platformSpecificFeatures = Ioc.Default.GetRequiredService<IPlatformSpecificFeatures>();
        if (platformSpecificFeatures.Platform == Platform.Windows)
        {
            var settingsService = Ioc.Default.GetRequiredService<SettingsPageService>();
            var appConfiguration = Ioc.Default.GetRequiredService<ILayeredAppConfiguration>();
            appConfiguration.User.Compatibility.CompatibilityTool = CompatibilityTool.Automatic;
            await settingsService.PersistCurrentConfigAsync();
        }
    }

    // This has to be called before MainWindowViewModel's actual construction, otherwise some strings won't be initialized properly
    private static void ApplyUiLanguage(ISettingsProvider settingsProvider)
    {
        var localizationManager = Ioc.Default.GetRequiredService<ILocalizationManager>();
        var applicationLanguage = settingsProvider.GetApplicationSettings().ApplicationLanguage;
        localizationManager.ChangeLanguage(applicationLanguage);
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

    private async Task ShowMainWindow(SplashScreen splashScreen, MainWindow mainWindow, KbUpdateResult kbUpdateResult)
    {
        splashScreen.Close();
        ApplyInitialSettings();
        mainWindow.Show();
        var updateService = Ioc.Default.GetRequiredService<UpdateService>();
        await updateService.StartAsync();
        await AppUtils.CheckCompatibilityToolAsync();
        if (kbUpdateResult.Severity == NotificationType.Error)
        {
            var notificationsService = Ioc.Default.GetRequiredService<NotificationService>();
            var notificationMessage = new StringBuilder()
                .AppendLine(kbUpdateResult.Message.GetLocalizedString());
            if (kbUpdateResult.Exception != null)
            {
                notificationMessage.AppendLine()
                    .AppendLine(kbUpdateResult.Exception.Message);
            }

            notificationMessage.AppendLine()
                .AppendLine("KB_ERROR_TRANSIENT_NOTICE");
            await notificationsService.AddWarningNotificationAsync("KB_UPDATE_ERROR", notificationMessage.ToString(),
                PackIconRemixIconKind.FileWarningLine);
        }
    }

    private async Task InitializeServices(Application application, ResourceDictionary resourceDictionary, IProgress<string> progress)
    {
        var platformSpecificFeatures = AppUtils.InitPlatformSpecificFeatures();

        var appDataDirectory = platformSpecificFeatures.GetAppDataDirectory();
        
        progress.Report("Reading application configuration...");

        var serviceCollection = new ServiceCollection();
        serviceCollection.AddAppConfiguration(appDataDirectory)
            .AddPlatformSpecificFeatures(platformSpecificFeatures)
            .AddTombLauncherLogging(appDataDirectory)
            .AddTombLauncherMappings()
            .RegisterAiFeatures()
            .AddSaveGameManagement()
            .AddTheming()
            .AddFileOperations()
            .AddPageServices()
            .AddViewModels()
            .AddLocalization(application, resourceDictionary)
            .AddDatabaseAccess(appDataDirectory)
            .AddPopups()
            .AddNavigation()
            .AddGameManagement()
            .AddDownloaders()
            .AddNotifications()
            .AddUpdater()
            .AddPatchers()
            .AddDiscordIntegration();

        var serviceProvider = serviceCollection.BuildServiceProvider();
        Ioc.Default.ConfigureServices(serviceProvider);
        Log.Logger.Information("Service initialization complete");

        await Task.CompletedTask;
    }

    private void ApplyInitialSettings()
    {
        var settingsProvider = Ioc.Default.GetRequiredService<ISettingsProvider>();
        var themeManager = Ioc.Default.GetRequiredService<ThemeManager>();

        var applicationTheme = settingsProvider.GetAppearanceSettings().ApplicationTheme;
        themeManager.ApplyTheme(applicationTheme);

        var baseVariant = ThemeVariant.Dark;
        if (!string.IsNullOrEmpty(applicationTheme) && applicationTheme.Contains("Light"))
        {
            baseVariant = ThemeVariant.Light;
        }
        AppUtils.ChangeTheme(baseVariant);
    }
}
