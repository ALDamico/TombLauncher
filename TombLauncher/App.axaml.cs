using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Threading;
using AutoMapper;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using CommunityToolkit.Mvvm.DependencyInjection;
using JamSoft.AvaloniaUI.Dialogs;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using TombLauncher.Contracts.Downloaders;
using TombLauncher.Contracts.Dtos;
using TombLauncher.Contracts.Localization;
using TombLauncher.Contracts.Localization.Dtos;
using TombLauncher.Contracts.Settings;
using TombLauncher.Core.Settings;
using TombLauncher.Data.Database.UnitOfWork;
using TombLauncher.Data.Models;
using TombLauncher.Installers;
using TombLauncher.Installers.Downloaders;
using TombLauncher.Installers.Downloaders.AspideTR.com;
using TombLauncher.Installers.Downloaders.TRLE.net;
using TombLauncher.Localization;
using TombLauncher.Navigation;
using TombLauncher.Services;
using TombLauncher.Utils;
using TombLauncher.ViewModels;
using TombLauncher.ViewModels.Pages;
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
            ConfigureMappings(serviceCollection);
            ConfigurePageServices(serviceCollection);
            ConfigureViewModels(serviceCollection);
            serviceCollection.AddSingleton<ILocalizationManager>(sp =>
            {
                var settingsVisitor = sp.GetRequiredService<ISettingsVisitor>();
                var locManager = new LocalizationManager(Current);
                //locManager.ChangeLanguage(CultureInfo.CurrentUICulture);
                
                locManager.Accept(settingsVisitor);
                return locManager;
            });
            ConfigureDatabaseAccess(serviceCollection);
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
                new TombLauncherGameMerger(new GameSearchResultMetadataDistanceCalculator(){UseAuthor = true, IgnoreSubTitle = true}));
            serviceCollection.AddScoped(sp =>
            {
                var cts = new CancellationTokenSource();
                var locMan = sp.GetService<ILocalizationManager>();
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
            
            

            serviceCollection.AddSingleton<ISettingsVisitor>(sp => new SettingsVisitorImpl(sp.GetRequiredService<SettingsUnitOfWork>()));
            
            var serviceProvider = serviceCollection.BuildServiceProvider();
            Ioc.Default.ConfigureServices(serviceProvider);
            var defaultPage = Ioc.Default.GetRequiredService<WelcomePageViewModel>();
            var navigationManager = Ioc.Default.GetRequiredService<NavigationManager>();
            navigationManager.SetDefaultPage(defaultPage);
            desktop.MainWindow = new MainWindow
            {
                DataContext = new MainWindowViewModel(navigationManager),
            };
        }

        base.OnFrameworkInitializationCompleted();
    }

    private static void ConfigurePageServices(ServiceCollection serviceCollection)
    {
        serviceCollection.AddScoped<GameDetailsService>();
        serviceCollection.AddScoped<NewGameService>();
        serviceCollection.AddScoped<GameListService>();
        serviceCollection.AddScoped<GameWithStatsService>();
        serviceCollection.AddScoped<AppCrashHostService>();
        serviceCollection.AddSingleton<WelcomePageService>();
        serviceCollection.AddScoped<GameSearchService>();
        serviceCollection.AddTransient<GameSearchResultService>();
        serviceCollection.AddScoped<SettingsService>();
    }

    private static void ConfigureViewModels(ServiceCollection serviceCollection)
    {
        serviceCollection.AddSingleton(sp =>
            new WelcomePageViewModel(sp.GetRequiredService<WelcomePageService>())
                { ChangeLogPath = "avares://TombLauncher/Data/CHANGELOG.md" });
        serviceCollection.AddScoped<GameListViewModel>();
        serviceCollection.AddScoped<GameSearchViewModel>();
        serviceCollection.AddScoped<NewGameViewModel>();
        serviceCollection.AddTransient<SettingsPageViewModel>();
    }

    private static void ConfigureDatabaseAccess(ServiceCollection serviceCollection)
    {
        serviceCollection.AddEntityFrameworkSqlite();
        serviceCollection.AddScoped<GamesUnitOfWork>();
        serviceCollection.AddScoped<AppCrashUnitOfWork>();
        serviceCollection.AddScoped<SettingsUnitOfWork>();
    }

    private static void ConfigureMappings(ServiceCollection serviceCollection)
    {
        var mapperConfiguration = new MapperConfiguration(cfg =>
        {
            cfg.AllowNullDestinationValues = true;
            
            cfg.CreateMap<AppCrash, AppCrashDto>()
                .ForMember(dto => dto.ExceptionDto,
                    opt => opt.MapFrom(s => JsonConvert.DeserializeObject<ExceptionDto>(s.Exception)));
            cfg.CreateMap<Exception, ExceptionDto>();

            cfg.CreateMap<GameHashes, GameHashDto>().ReverseMap();
            cfg.CreateMap<GameLink, GameLinkDto>().ReverseMap();
            cfg.CreateMap<Game, GameMetadataDto>().ReverseMap();
            cfg.CreateMap<AvailableLanguageDto, ApplicationLanguageViewModel>()
                .ForMember(dto => dto.CultureInfo, opt => opt.MapFrom(culture => culture.Culture)).ReverseMap();
            cfg.CreateMap<PlaySession, PlaySessionDto>().ReverseMap();
            cfg.CreateMap<IGameSearchResultMetadata, GameSearchResultMetadataViewModel>()
                .ForMember(dto => dto.TitlePic, opt => opt.MapFrom(dto => ImageUtils.ToBitmap(dto.TitlePic)));
            cfg.CreateMap<GameSearchResultMetadataViewModel, IGameSearchResultMetadata>()
                .ConstructUsing(vm => new GameSearchResultMetadataDto())
                .ForMember(dto => dto.TitlePic, opt => opt.MapFrom(vm => ImageUtils.ToByteArray(vm.TitlePic)));
            cfg.CreateMap<GameMetadataDto, GameMetadataViewModel>()
                .ForMember(dto => dto.TitlePic, opt => opt.MapFrom(dto => ImageUtils.ToBitmap(dto.TitlePic)));
            cfg.CreateMap<GameMetadataViewModel, GameMetadataDto>()
                .ForMember(dto => dto.TitlePic, opt => opt.MapFrom(vm => ImageUtils.ToByteArray(vm.TitlePic)));
            cfg.CreateMap<IMultiSourceSearchResultMetadata, MultiSourceGameSearchResultMetadataViewModel>()
                .ConstructUsing(vm => new MultiSourceGameSearchResultMetadataViewModel(Ioc.Default.GetService<GameSearchResultService>()))
                .ForMember(dto => dto.TitlePic, opt => opt.MapFrom(dto => ImageUtils.ToBitmap(dto.TitlePic)));
            cfg.CreateMap<MultiSourceGameSearchResultMetadataViewModel, IMultiSourceSearchResultMetadata>()
                .ConstructUsing(vm => new MultiSourceSearchResultMetadataDto())
                .ForMember(dto => dto.TitlePic, opt => opt.MapFrom(vm => ImageUtils.ToByteArray(vm.TitlePic)));
            cfg.CreateMap<MultiSourceGameSearchResultMetadataViewModel, GameSearchResultMetadataDto>()
                .ForMember(vm => vm.TitlePic, opt => opt.MapFrom(dto => ImageUtils.ToByteArray(dto.TitlePic)));
        });

        serviceCollection.AddSingleton(_ => mapperConfiguration);
    }
}