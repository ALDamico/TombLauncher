using System;
using AutoMapper;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using Avalonia.Styling;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.DependencyInjection;
using JamSoft.AvaloniaUI.Dialogs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NetSparkleUpdater;
using NetSparkleUpdater.Enums;
using NetSparkleUpdater.SignatureVerifiers;
using Serilog;
using Serilog.Events;
using TombLauncher.Configuration;
using TombLauncher.Contracts.Localization;
using TombLauncher.Core.Exceptions;
using TombLauncher.Core.PlatformSpecific;
using TombLauncher.Core.Savegames;
using TombLauncher.Data.Database.Repositories;
using TombLauncher.Data.Database;
using TombLauncher.Data.Database.Services;
using TombLauncher.Data.Database.UnitOfWork;
using TombLauncher.Factories;
using TombLauncher.Installers;
using TombLauncher.Installers.Downloaders;
using TombLauncher.Installers.Downloaders.AspideTR.com;
using TombLauncher.Installers.Downloaders.TRCustoms.org;
using TombLauncher.Installers.Downloaders.TRLE.net;
using TombLauncher.Localization;
using TombLauncher.Services;
using TombLauncher.Updater;
using TombLauncher.Utils;
using TombLauncher.ViewModels;
using TombLauncher.ViewModels.Pages;
using TombLauncher.Views;
using StringNotificationViewModel = TombLauncher.ViewModels.Notifications.StringNotificationViewModel;

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

    private SparkleUpdater _sparkle = null!;

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
        await ApplyInitialSettings();
        var navigationManager = Ioc.Default.GetRequiredService<NavigationManager>();
        var mainWindow = new MainWindow
        {
            DataContext = new MainWindowViewModel(navigationManager,
                Ioc.Default.GetRequiredService<NotificationListViewModel>(), Ioc.Default.GetRequiredService<NotificationService>(),
                Ioc.Default.GetRequiredService<ISettingsProvider>(), Ioc.Default.GetRequiredService<IPlatformSpecificFeatures>()),
        };

        desktop.MainWindow = mainWindow;
        desktop.ShutdownMode = Avalonia.Controls.ShutdownMode.OnMainWindowClose;
        mainWindow.Show();
        splashScreen.Close();
        var appConfiguration = Ioc.Default.GetRequiredService<IAppConfigurationWrapper>();
        await InitNetSparkle(appConfiguration);
        await _sparkle.CheckForUpdatesQuietly();
        await Task.CompletedTask;
    }

    private async Task InitializeServices()
    {
        var appConfiguration = new AppConfigurationWrapper();
        IConfiguration configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .Build();
        configuration.Bind(appConfiguration.Defaults);
        IConfiguration userConfiguration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.user.json", optional: true)
            .Build();
        userConfiguration.Bind(appConfiguration.User);

        var serviceCollection = new ServiceCollection();
        serviceCollection.AddSingleton<IAppConfigurationWrapper>(appConfiguration);
        serviceCollection.AddSingleton<IPlatformSpecificFeatures>(_ =>
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return new WindowsPlatformSpecificFeatures();
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                return new LinuxPlatformSpecificFeatures();
            }

            throw new PlatformNotSupportedException("This platform is not supported.");
        });
        ConfigureLogging(serviceCollection);
        ConfigureMappings(serviceCollection);
        serviceCollection.AddSingleton<IAppFileOperationsService, AppFileOperationsService>();
        serviceCollection.AddSingleton<ThemeManager>();
        serviceCollection.AddSingleton<ISavegameHeaderProvider, SavegameHeaderProvider>();
        serviceCollection.AddTransient<SavegameQueryService>();
        serviceCollection.AddTransient<SavegameCommandService>();
        ConfigurePageServices(serviceCollection);
        ConfigureViewModels(serviceCollection);
        serviceCollection.AddSingleton<ILocalizationManager>(_ => new LocalizationManager(Current!));
        ConfigureDatabaseAccess(serviceCollection, appConfiguration);
        serviceCollection.AddSingleton(sp => new NavigationManager(sp));
        serviceCollection.AddScoped(_ => DialogServiceFactory.Create(new DialogServiceConfiguration()
        {
            ApplicationName = "Tomb Launcher",
            UseApplicationNameInTitle = true,
            ViewsAssemblyName = Assembly.GetExecutingAssembly().GetName().Name
        }));
        serviceCollection.AddScoped(_ => DialogServiceFactory.CreateMessageBoxService());
        serviceCollection.AddScoped<TombRaiderLevelInstaller>();
        serviceCollection.AddScoped<TombRaiderEngineDetector>();
        serviceCollection.AddTransient<IGameMerger>(_ =>
            new TombLauncherGameMerger(new GameSearchResultMetadataDistanceCalculator()
            { UseAuthor = true, IgnoreSubTitle = true }));
        ConfigureDownloaders(serviceCollection);
        serviceCollection.AddTransient(sp =>
        {
            var cts = new CancellationTokenSource();
            var downloadManager = new GameDownloadManager(cts, sp.GetRequiredService<IGameMerger>())
            {
                Downloaders = Ioc.Default.GetRequiredService<ISettingsProvider>().GetActiveDownloaders()
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


        serviceCollection.AddSingleton<NotificationService>();
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

    private async Task InitNetSparkle(IAppConfigurationWrapper appConfiguration)
    {
        var updateWorkers = UpdateUtils.AppCastWorkersFactory(appConfiguration);

        _sparkle = new SparkleUpdater(appConfiguration.AppCastUrl ?? string.Empty,
            new Ed25519Checker(SecurityMode.Strict, appConfiguration.AppCastPublicKey))
        {
            UIFactory = updateWorkers.UiFactory(),
            AppCastDataDownloader = updateWorkers.AppCastDataDownloader,
            UpdateDownloader = updateWorkers.UpdateDownloader,
            LogWriter = updateWorkers.LoggerToUse,
            UserInteractionMode = UserInteractionMode.NotSilent,
            AppCastHelper = updateWorkers.AppCastHelper
        };
        _sparkle.UpdateDetected += (_, args) =>
        {
            var payload = new UpdateCommandPayload(_sparkle, args);
            var notificationService = Ioc.Default.GetRequiredService<NotificationService>();
            var localizationService = Ioc.Default.GetRequiredService<ILocalizationManager>();
            notificationService.AddNotification(new NotificationViewModel()
            {
                Content = new StringNotificationViewModel()
                { Text = localizationService.GetLocalizedString($"Update available notification", args.LatestVersion.Version ?? "") },
                IsDismissable = true,
                IsCancelable = false,
                IsOpenable = true,
                OpenCommand = updateWorkers.UpdateCommand,
                OpenIcon = updateWorkers.UpdateIcon,
                OpenCmdParam = payload
            });
        };
        await _sparkle.StartLoop(true);
    }

    private Task ApplyInitialSettings()
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

        return Task.CompletedTask;
    }

    private void ConfigureDownloaders(ServiceCollection serviceCollection)
    {
        serviceCollection.AddTransient<TrleGameDownloader>();
        serviceCollection.AddTransient(sp => new AspideTrGameDownloader(sp.GetRequiredService<ILocalizationManager>()
            .GetSubsetInvertedByPrefix("ATR")));
        serviceCollection.AddTransient<TrCustomsGameDownloader>();
    }

    private static void ConfigurePageServices(ServiceCollection serviceCollection)
    {
        serviceCollection.AddSingleton<ViewServiceContext>();
        serviceCollection.AddScoped<GameDetailsService>();
        serviceCollection.AddScoped<NewGameService>();
        serviceCollection.AddScoped<GameListService>();
        serviceCollection.AddScoped<GameWithStatsService>();
        serviceCollection.AddScoped<AppCrashHostService>();
        serviceCollection.AddSingleton<WelcomePageService>();
        serviceCollection.AddTransient<GameSearchService>();
        serviceCollection.AddTransient<GameSearchResultService>();
        serviceCollection.AddSingleton<ISettingsProvider, SettingsProvider>();
        serviceCollection.AddSingleton<SettingsPageService>();
        serviceCollection.AddTransient<RandomGameService>();
        serviceCollection.AddScoped<StatisticsService>();
    }

    private static void ConfigureViewModels(ServiceCollection serviceCollection)
    {
        serviceCollection.AddSingleton(sp =>
            new WelcomePageViewModel(sp.GetRequiredService<WelcomePageService>())
            { ChangeLogPath = "avares://TombLauncher/Data/CHANGELOG.md" });
        serviceCollection.AddScoped<GameListViewModel>();
        serviceCollection.AddScoped<GameSearchViewModel>();
        serviceCollection.AddTransient<NewGameViewModel>();
        serviceCollection.AddSingleton<SettingsPageViewModel>();
        serviceCollection.AddSingleton<NotificationListViewModel>();
        serviceCollection.AddTransient<RandomGameViewModel>();
        serviceCollection.AddScoped<StatisticsPageViewModel>();
        serviceCollection.AddTransient<SavegameListViewModel>();
        serviceCollection.AddTransient<GameDetailsViewModel>();
    }

    private static void ConfigureDatabaseAccess(ServiceCollection serviceCollection, IAppConfiguration appConfiguration)
    {
        serviceCollection.AddDbContext<TombLauncherDbContext>(opts =>
        {
            var databasePath = appConfiguration.DatabasePath;
            var connectionString = $"Data Source={databasePath}";
            opts.UseSqlite(connectionString);
        });
        serviceCollection.AddScoped<ISavegameRepository, SavegameRepository>();
        serviceCollection.AddScoped<GamesUnitOfWork>();
        serviceCollection.AddScoped<AppCrashDataService>();
    }

    private static void ConfigureLogging(ServiceCollection serviceCollection)
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .WriteTo
            .File("TombLauncher_App.log", LogEventLevel.Information)
            .CreateLogger();
        serviceCollection.AddLogging(opts =>
        {
            opts.ClearProviders();
            opts.SetMinimumLevel(LogLevel.Information);
            opts.AddSerilog();
        });
    }

    private static void ConfigureMappings(ServiceCollection serviceCollection)
    {
        serviceCollection.AddSingleton(sp => MapperConfigurationFactory.GetMapperConfiguration(t => sp.GetService(t)!));
        serviceCollection.AddSingleton<IMapper>(sp =>
        {
            var config = sp.GetRequiredService<MapperConfiguration>();
            return config.CreateMapper(t => sp.GetService(t)!);
        });
    }
}