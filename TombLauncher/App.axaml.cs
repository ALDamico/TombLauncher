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
using TombLauncher.Database.UnitOfWork;
using TombLauncher.Installers;
using TombLauncher.Installers.Downloaders;
using TombLauncher.Installers.Downloaders.AspideTR.com;
using TombLauncher.Installers.Downloaders.TRLE.net;
using TombLauncher.Localization;
using TombLauncher.Navigation;
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
            serviceCollection.AddSingleton(sp =>
            {
                var locManager = new LocalizationManager(Current);
                locManager.ChangeLanguage(CultureInfo.CurrentUICulture);
                return locManager;
            });
            serviceCollection.AddEntityFrameworkSqlite();
            serviceCollection.AddSingleton(sp => new WelcomePageViewModel(Ioc.Default.GetRequiredService<LocalizationManager>())
                { ChangeLogPath = "avares://TombLauncher/Data/CHANGELOG.md" });
            serviceCollection.AddScoped<GamesUnitOfWork>();
            serviceCollection.AddScoped<GameListViewModel>();
            serviceCollection.AddSingleton(sp =>
            {
                var defaultPage = sp.GetRequiredService<WelcomePageViewModel>();
                return new NavigationManager(defaultPage);
            });
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
            serviceCollection.AddScoped(sp =>
            {
                var cts = sp.GetService<CancellationTokenSource>();
                var locMan = sp.GetService<LocalizationManager>();
                return new GameDownloadManager(cts)
                {
                    Downloaders =
                    {
                        new TrleGameDownloader(sp.GetService<TombRaiderLevelInstaller>(),
                            sp.GetService<TombRaiderEngineDetector>(), cts),
                        new AspideTrGameDownloader(locMan.GetSubsetInvertedByPrefix("ATR"))
                    }
                };
            });
            var serviceProvider = serviceCollection.BuildServiceProvider();
            Ioc.Default.ConfigureServices(serviceProvider);
            desktop.MainWindow = new MainWindow
            {
                DataContext = new MainWindowViewModel(Ioc.Default.GetRequiredService<GamesUnitOfWork>(),
                    Ioc.Default.GetRequiredService<NavigationManager>(),
                    Ioc.Default.GetRequiredService<LocalizationManager>()),
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
}