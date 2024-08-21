using System;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using CommunityToolkit.Mvvm.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using TombLauncher.Database.UnitOfWork;
using TombLauncher.ViewModels;
using TombLauncher.ViewModels.ViewModels;
using TombLauncher.Views;

namespace TombLauncher;

public partial class App : Application
{
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
            serviceCollection.AddEntityFrameworkSqlite();
            serviceCollection.AddScoped<GamesUnitOfWork>();
            serviceCollection.AddScoped<GameListViewModel>();
            var serviceProvider = serviceCollection.BuildServiceProvider();
            Ioc.Default.ConfigureServices(serviceProvider);
            desktop.MainWindow = new MainWindow
            {
                DataContext = new MainWindowViewModel(Ioc.Default.GetRequiredService<GamesUnitOfWork>()),
            };
        }

        base.OnFrameworkInitializationCompleted();
    }

    private IServiceProvider _serviceProvider;

    public static IServiceProvider GetServiceProvider()
    {
        return (Current as App)?._serviceProvider;
    }
}