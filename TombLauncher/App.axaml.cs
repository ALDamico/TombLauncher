using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Threading;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using CommunityToolkit.Mvvm.DependencyInjection;
using JamSoft.AvaloniaUI.Dialogs;
using Microsoft.Extensions.DependencyInjection;
using TombLauncher.Data.Database.UnitOfWork;
using TombLauncher.Installers;
using TombLauncher.Installers.Downloaders;
using TombLauncher.Installers.Downloaders.AspideTR.com;
using TombLauncher.Installers.Downloaders.TRLE.net;
using TombLauncher.Localization;
using TombLauncher.Navigation;
using TombLauncher.Services;
using TombLauncher.ViewModels;
using TombLauncher.Views;

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

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // Line below is needed to remove Avalonia data validation.
            // Without this line you will get duplicate validations from both Avalonia and CT
            BindingPlugins.DataValidators.RemoveAt(0);

            var serviceCollection = new ServiceCollection();
            serviceCollection.AddScoped<GameDetailsService>();
            serviceCollection.AddScoped<NewGameService>();
            serviceCollection.AddScoped<NewGameViewModel>();
            serviceCollection.AddScoped<GameListService>();
            serviceCollection.AddScoped<GameWithStatsService>();
            serviceCollection.AddScoped<AppCrashHostService>();
            serviceCollection.AddSingleton<WelcomePageService>();
            serviceCollection.AddSingleton(sp =>
            {
                var locManager = new LocalizationManager(Current);
                locManager.ChangeLanguage(CultureInfo.CurrentUICulture);
                return locManager;
            });
            serviceCollection.AddEntityFrameworkSqlite();
            serviceCollection.AddSingleton(sp =>
                new WelcomePageViewModel(sp.GetRequiredService<WelcomePageService>())
                    { ChangeLogPath = "avares://TombLauncher/Data/CHANGELOG.md" });
            serviceCollection.AddScoped<GamesUnitOfWork>();
            serviceCollection.AddScoped<GameListViewModel>();
            serviceCollection.AddSingleton(sp => new NavigationManager());
            serviceCollection.AddScoped(_ => DialogServiceFactory.Create(new DialogServiceConfiguration()
            {
                ApplicationName = "Tomb Launcher",
                UseApplicationNameInTitle = true,
                ViewsAssemblyName = Assembly.GetExecutingAssembly().GetName().Name
            }));
            serviceCollection.AddScoped(_ => DialogServiceFactory.CreateMessageBoxService());
            serviceCollection.AddScoped<GameSearchViewModel>();
            serviceCollection.AddScoped<TombRaiderLevelInstaller>();
            serviceCollection.AddScoped<TombRaiderEngineDetector>();
            serviceCollection.AddScoped<CancellationTokenSource>();
            serviceCollection.AddTransient<IGameMerger>(_ =>
                new TombLauncherGameMerger(new GameSearchResultMetadataDistanceCalculator(){UseAuthor = true, IgnoreSubTitle = true}));
            serviceCollection.AddScoped(sp =>
            {
                var cts = sp.GetService<CancellationTokenSource>();
                var locMan = sp.GetService<LocalizationManager>();
                return new GameDownloadManager(cts, sp.GetRequiredService<IGameMerger>())
                {
                    Downloaders =
                    {
                        new TrleGameDownloader(sp.GetService<TombRaiderLevelInstaller>(),
                            sp.GetService<TombRaiderEngineDetector>(), cts),
                        new AspideTrGameDownloader(locMan.GetSubsetInvertedByPrefix("ATR"))
                    }
                };
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
            serviceCollection.AddScoped<AppCrashUnitOfWork>();
            var serviceProvider = serviceCollection.BuildServiceProvider();
            Ioc.Default.ConfigureServices(serviceProvider);
            var defaultPage = Ioc.Default.GetRequiredService<WelcomePageViewModel>();
            var navigationManager = Ioc.Default.GetRequiredService<NavigationManager>();
            navigationManager.SetDefaultPage(defaultPage);
            desktop.MainWindow = new MainWindow
            {
                DataContext = new MainWindowViewModel(navigationManager,
                    Ioc.Default.GetRequiredService<LocalizationManager>()),
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
}