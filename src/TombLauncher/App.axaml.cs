using System;
using System.Reflection;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using TombLauncher.Core.PlatformSpecific;
using TombLauncher.Data.Database;
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

                Dispatcher.UIThread.UnhandledException += AppUtils.OnUnhandledException;

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

    private async Task ShowMainWindow(IClassicDesktopStyleApplicationLifetime desktop, SplashScreen splashScreen)
    {
        // Services initialized in background task
        AppUtils.ApplyInitialSettings();
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
        await AppUtils.CheckCompatibilityToolAsync();
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
}