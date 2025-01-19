using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using Avalonia.Styling;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using JamSoft.AvaloniaUI.Dialogs;
using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView;
using Material.Icons;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NetSparkleUpdater;
using NetSparkleUpdater.Downloaders;
using NetSparkleUpdater.Enums;
using NetSparkleUpdater.SignatureVerifiers;
using NetSparkleUpdater.UI.Avalonia;
using Serilog;
using Serilog.Events;
using TombLauncher.Configuration;
using TombLauncher.Contracts.Localization;
using TombLauncher.Data.Database;
using TombLauncher.Data.Database.UnitOfWork;
using TombLauncher.Factories;
using TombLauncher.Installers;
using TombLauncher.Installers.Downloaders;
using TombLauncher.Installers.Downloaders.AspideTR.com;
using TombLauncher.Installers.Downloaders.TRLE.net;
using TombLauncher.Localization;
using TombLauncher.Localization.Extensions;
using TombLauncher.Navigation;
using TombLauncher.Services;
using TombLauncher.Updater;
using TombLauncher.Utils;
using TombLauncher.ViewModels;
using TombLauncher.ViewModels.Pages;
using TombLauncher.Views;
using StringNotificationViewModel = TombLauncher.ViewModels.Notifications.StringNotificationViewModel;

namespace TombLauncher;

public partial class App : Application
{
    public App()
    {
        Name = Assembly.GetExecutingAssembly().GetName().Name;
    }

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private SparkleUpdater _sparkle;

    public override async void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // Line below is needed to remove Avalonia data validation.
            // Without this line you will get duplicate validations from both Avalonia and CT
            BindingPlugins.DataValidators.RemoveAt(0);
            var splashScreen = new SplashScreen();
            desktop.MainWindow = splashScreen;
            splashScreen.Show();
            desktop.ShutdownRequested += (sender, args) =>
            {
                Log.Logger.Information("Application is shutting down");
                ((IDisposable)Log.Logger).Dispose();
            };
            
            await Task.Delay(1);
            await InitializeServices();
            await ShowMainWindow(desktop, splashScreen);
        }

        base.OnFrameworkInitializationCompleted();
    }

    private async Task ShowMainWindow(IClassicDesktopStyleApplicationLifetime desktop, SplashScreen splashScreen)
    {
        ApplyInitialSettings();
        var defaultPage = Ioc.Default.GetRequiredService<WelcomePageViewModel>();
        var navigationManager = Ioc.Default.GetRequiredService<NavigationManager>();
        navigationManager.SetDefaultPage(defaultPage);
        var mainWindow = new MainWindow
        {
            DataContext = new MainWindowViewModel(navigationManager,
                Ioc.Default.GetRequiredService<NotificationListViewModel>()),
        };

        desktop.MainWindow = mainWindow;
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
        ConfigureLogging(serviceCollection);
        ConfigureMappings(serviceCollection);
        ConfigurePageServices(serviceCollection);
        ConfigureViewModels(serviceCollection);
        serviceCollection.AddSingleton<ILocalizationManager>(_ => new LocalizationManager(Current));
        ConfigureDatabaseAccess(serviceCollection, appConfiguration);
        serviceCollection.AddSingleton(_ => new NavigationManager());
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
                Downloaders = Ioc.Default.GetRequiredService<SettingsService>().GetActiveDownloaders()
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

        var serviceProvider = serviceCollection.BuildServiceProvider();
        Ioc.Default.ConfigureServices(serviceProvider);
        Log.Logger.Information("Service initialization complete");
        
        await Task.CompletedTask;
    }

    private async Task InitNetSparkle(IAppConfigurationWrapper appConfiguration)
    {
        var updateWorkers = AppCastWorkersFactory(appConfiguration);

        _sparkle = new SparkleUpdater(appConfiguration.AppCastUrl,
            new Ed25519Checker(SecurityMode.Strict, appConfiguration.AppCastPublicKey))
        {
            UIFactory = new UIFactory(),
            AppCastDataDownloader = updateWorkers.AppCastDataDownloader,
            UpdateDownloader = updateWorkers.UpdateDownloader,
            LogWriter = updateWorkers.LoggerToUse,
            UserInteractionMode = UserInteractionMode.NotSilent,
        };
        _sparkle.UpdateDetected += (sender, args) =>
        {
            var notificationService = Ioc.Default.GetRequiredService<NotificationService>();
            var localizationService = Ioc.Default.GetRequiredService<ILocalizationManager>();
            notificationService.AddNotification(new NotificationViewModel()
            {
                Content = new StringNotificationViewModel()
                    { Text = localizationService.GetLocalizedString($"Update available notification", args.LatestVersion.Version) },
                IsDismissable = true, IsCancelable = false, IsOpenable = true,
                OpenCommand =
                    new RelayCommand( () =>  _sparkle.ShowUpdateNeededUI(args.AppCastItems)),
                OpenIcon = MaterialIconKind.Download
            });
        };
        await _sparkle.StartLoop(true);
    }

    private static UpdaterWorkersPayload AppCastWorkersFactory(IAppConfigurationWrapper appConfiguration)
    {
        UpdaterWorkersPayload updateWorkers = new UpdaterWorkersPayload()
        {
            LoggerToUse = new SerilogLogWriter()
        };
        if (appConfiguration.UpdaterUseLocalPaths)
        {
            updateWorkers.AppCastDataDownloader = new LocalFileAppCastDownloader() { UseLocalUriPath = true };
            updateWorkers.UpdateDownloader = new LocalFileDownloader(updateWorkers.LoggerToUse) { UseLocalUriPath = true };
        }
        else
        {
            updateWorkers.AppCastDataDownloader = new WebRequestAppCastDataDownloader(updateWorkers.LoggerToUse);
            updateWorkers.UpdateDownloader = new WebFileDownloader(updateWorkers.LoggerToUse);
        }

        return updateWorkers;
    }

    private void ApplyInitialSettings()
    {
        var settingsService = Ioc.Default.GetRequiredService<SettingsService>();
        var localizationManager = Ioc.Default.GetRequiredService<ILocalizationManager>();
        var applicationLanguage = settingsService.GetApplicationLanguage();
        localizationManager.ChangeLanguage(applicationLanguage);
        var applicationTheme = settingsService.GetApplicationTheme();
        AppUtils.ChangeTheme(applicationTheme);
    }

    private void ConfigureDownloaders(ServiceCollection serviceCollection)
    {
        serviceCollection.AddTransient<TrleGameDownloader>();
        serviceCollection.AddTransient(sp => new AspideTrGameDownloader(sp.GetRequiredService<ILocalizationManager>()
            .GetSubsetInvertedByPrefix("ATR")));
    }

    private static void ConfigurePageServices(ServiceCollection serviceCollection)
    {
        serviceCollection.AddScoped<GameDetailsService>();
        serviceCollection.AddScoped<NewGameService>();
        serviceCollection.AddScoped<GameListService>();
        serviceCollection.AddScoped<GameWithStatsService>();
        serviceCollection.AddScoped<AppCrashHostService>();
        serviceCollection.AddSingleton<WelcomePageService>();
        serviceCollection.AddTransient<GameSearchService>();
        serviceCollection.AddTransient<GameSearchResultService>();
        serviceCollection.AddSingleton<SettingsService>();
        serviceCollection.AddTransient<RandomGameService>();
        serviceCollection.AddScoped<StatisticsService>();
        serviceCollection.AddTransient<SavegameService>();
    }

    private static void ConfigureViewModels(ServiceCollection serviceCollection)
    {
        serviceCollection.AddSingleton(sp =>
            new WelcomePageViewModel()
                { ChangeLogPath = "avares://TombLauncher/Data/CHANGELOG.md" });
        serviceCollection.AddScoped<GameListViewModel>();
        serviceCollection.AddScoped<GameSearchViewModel>();
        serviceCollection.AddTransient<NewGameViewModel>();
        serviceCollection.AddSingleton<SettingsPageViewModel>();
        serviceCollection.AddSingleton<NotificationListViewModel>();
        serviceCollection.AddTransient<RandomGameViewModel>();
        serviceCollection.AddScoped<StatisticsPageViewModel>();
        serviceCollection.AddTransient<SavegameListViewModel>();
    }

    private static void ConfigureDatabaseAccess(ServiceCollection serviceCollection, IAppConfiguration appConfiguration)
    {
        serviceCollection.AddDbContext<TombLauncherDbContext>(opts =>
        {
            var databasePath = appConfiguration.DatabasePath;
            var connectionString = $"Data Source={databasePath}";
            opts.UseSqlite(connectionString);
        });
        serviceCollection.AddScoped<GamesUnitOfWork>();
        serviceCollection.AddScoped<AppCrashUnitOfWork>();
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
        serviceCollection.AddSingleton(_ => MapperConfigurationFactory.GetMapperConfiguration());
    }
}