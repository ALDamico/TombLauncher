using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
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
using JamSoft.AvaloniaUI.Dialogs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using TombLauncher.Configuration;
using TombLauncher.Configuration.Sections;
using TombLauncher.Contracts.Localization;
using TombLauncher.Core.Exceptions;
using TombLauncher.Core.Launchers;
using TombLauncher.Core.PlatformSpecific;
using TombLauncher.Core.Savegames;
using TombLauncher.Data.Database;
using TombLauncher.Data.Database.Services;
using TombLauncher.Extensions;
using TombLauncher.Installers;
using TombLauncher.Installers.Downloaders;
using TombLauncher.Localization;
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
                    Version = AppUtils.GetApplicationVersion()
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

                try
                {
                    // Run initialization in background to allow Splash Screen to render
                    var mainWindowVm = await Task.Run(async () =>
                    {
                        await InitializeServices(Current!, resourceDictionary, progress);

                        progress.Report("Restoring language settings...");
                        var settingsProvider = Ioc.Default.GetRequiredService<ISettingsProvider>();
                        Dispatcher.UIThread.Invoke(() => { ApplyUiLanguage(settingsProvider); });

                        progress.Report("Updating application database...");
                        using var scope = Ioc.Default.CreateScope();
                        var dbContext = scope.ServiceProvider.GetRequiredService<TombLauncherDbContext>();
                        await dbContext.Database.MigrateAsync();
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

                    await Dispatcher.UIThread.InvokeAsync(async () => await ShowMainWindow(splashScreen, mainWindow));
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

    private async Task ShowMainWindow(SplashScreen splashScreen, MainWindow mainWindow)
    {
        splashScreen.Close();
        ApplyInitialSettings();
        mainWindow.Show();
        var updateService = Ioc.Default.GetRequiredService<UpdateService>();
        await updateService.StartAsync();
        await CheckCompatibilityToolAsync();
    }

    private async Task InitializeServices(Application application, ResourceDictionary resourceDictionary, IProgress<string> progress)
    {
        var platformSpecificFeatures = AppUtils.InitPlatformSpecificFeatures();

        var appDataDirectory = platformSpecificFeatures.GetAppDataDirectory();
        
        progress.Report("Reading application configuration...");

        var appConfiguration = new LayeredAppConfiguration();
        IConfiguration configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .Build();
        configuration.Bind(appConfiguration.Defaults);
        var userConfigPath = Path.Combine(appDataDirectory, "appsettings.user.json");
        IConfiguration userConfiguration = new ConfigurationBuilder()
            .AddJsonFile(userConfigPath, optional: true)
            .Build();
        userConfiguration.Bind(appConfiguration.User);

        var serviceCollection = new ServiceCollection();
        serviceCollection.AddSingleton<ILayeredAppConfiguration>(appConfiguration);
        serviceCollection.AddSingleton<IAppConfiguration>(sp => sp.GetRequiredService<ILayeredAppConfiguration>());
        serviceCollection.AddSingleton(platformSpecificFeatures);
        serviceCollection.AddTombLauncherLogging(appDataDirectory)
            .AddTombLauncherMappings();
        serviceCollection.AddSingleton<IAppFileOperationsService, AppFileOperationsService>();
        serviceCollection.AddSingleton<ThemeManager>();
        serviceCollection.AddSingleton<ISavegameHeaderProvider, SavegameHeaderProvider>();
        serviceCollection.AddTransient<SavegameQueryService>();
        serviceCollection.AddTransient<SavegameCommandService>();
        serviceCollection.AddPageServices()
            .AddViewModels();
        serviceCollection.AddSingleton<ILocalizationManager>(_ => new LocalizationManager(Current!, resourceDictionary));
        serviceCollection.AddDatabaseAccess(appConfiguration, appDataDirectory);
        serviceCollection.AddSingleton(sp => new NavigationManager(sp));
        serviceCollection.AddSingleton<IPopupService>(_ => new PopupService(
            DialogServiceFactory.CreateMessageBoxService(),
            DialogServiceFactory.Create(new DialogServiceConfiguration()
            {
                ApplicationName = "Tomb Launcher",
                UseApplicationNameInTitle = true,
                ViewsAssemblyName = Assembly.GetExecutingAssembly().GetName().Name
            })));
        serviceCollection.AddScoped<TombRaiderLevelInstaller>();
        serviceCollection.AddScoped<TombRaiderEngineDetector>();
        serviceCollection.AddTransient<IGameMerger>(_ =>
            new TombLauncherGameMerger(new GameSearchResultMetadataDistanceCalculator()
            { UseAuthor = true, IgnoreSubTitle = true }));
        serviceCollection.AddDownloaders();
        serviceCollection.AddTransient(sp =>
        {
            var downloadManager = new GameDownloadManager(sp.GetRequiredService<IGameMerger>())
            {
                Downloaders = sp.GetRequiredService<ISettingsProvider>().GetActiveDownloaders()
            };


            return downloadManager;
        });
        serviceCollection.AddScoped(_ => new GameFileHashCalculator(new HashSet<string>()
        {
            ".tr4",
            ".pak",
            ".tr2",
            ".sfx",
            ".dat",
            ".phd"
        }));


        serviceCollection.AddTransient<IGameLauncher>(sp =>
        {
            var cfg = sp.GetRequiredService<IAppConfiguration>().Compatibility;
            return cfg.CompatibilityTool switch
            {
                CompatibilityTool.Proton => new ProtonGameLauncher(cfg.ProtonPath ?? ""),
                CompatibilityTool.None => new WindowsGameLauncher(),
                _ => new WineGameLauncher(cfg.WinePath ?? "wine"),
            };
        });

        serviceCollection.AddSingleton<NotificationService>();
        serviceCollection.AddSingleton<UpdateService>();
        serviceCollection.AddScoped(sp =>
        {
            var settingsProvider = sp.GetRequiredService<ISettingsProvider>();
            var delay = settingsProvider.GetSavegameSettings().ProcessingDelay;
            var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
            return new SavegameHeaderProcessor(loggerFactory.CreateLogger<SavegameHeaderProcessor>())
            {
                Delay = delay
            };
        });

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
