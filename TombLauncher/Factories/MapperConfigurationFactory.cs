using System;
using AutoMapper;
using CommunityToolkit.Mvvm.DependencyInjection;
using Newtonsoft.Json;
using TombLauncher.Contracts.Downloaders;
using TombLauncher.Contracts.Localization.Dtos;
using TombLauncher.Core.Dtos;
using TombLauncher.Data.Models;
using TombLauncher.Services;
using TombLauncher.Utils;
using TombLauncher.ViewModels;
using TombLauncher.ViewModels.Pages.Settings;

namespace TombLauncher.Factories;

public static class MapperConfigurationFactory
{
    public static MapperConfiguration GetMapperConfiguration()
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
            cfg.CreateMap<IGameSearchResultMetadata, GameSearchResultMetadataViewModel>();
            cfg.CreateMap<GameSearchResultMetadataViewModel, IGameSearchResultMetadata>()
                .ConstructUsing(vm => new GameSearchResultMetadataDto())
                .ForMember(dto => dto.TitlePic, opt => opt.MapFrom(vm => ImageUtils.ToByteArray(vm.TitlePic)))
                .ForMember(dto => dto.SourceSiteDisplayName, opt => opt.Ignore());
            cfg.CreateMap<GameMetadataDto, GameMetadataViewModel>()
                .ForMember(dto => dto.TitlePic, opt => opt.MapFrom(dto => ImageUtils.ToBitmap(dto.TitlePic)));
            cfg.CreateMap<GameMetadataViewModel, GameMetadataDto>()
                .ForMember(dto => dto.TitlePic, opt => opt.MapFrom(vm => ImageUtils.ToByteArray(vm.TitlePic)));
            cfg.CreateMap<IMultiSourceSearchResultMetadata, MultiSourceGameSearchResultMetadataViewModel>()
                .ConstructUsing(vm =>
                    new MultiSourceGameSearchResultMetadataViewModel(Ioc.Default
                        .GetService<GameSearchResultService>()))
                .ForMember(vm => vm.InstallProgress, m => m.Ignore())
                .ForMember(vm => vm.InstalledGame, m => m.Ignore())
                ;
            cfg.CreateMap<MultiSourceGameSearchResultMetadataViewModel, IMultiSourceSearchResultMetadata>()
                .ConstructUsing(vm => new MultiSourceSearchResultMetadataDto());
            cfg.CreateMap<MultiSourceGameSearchResultMetadataViewModel, GameSearchResultMetadataDto>();
            cfg.CreateMap<DownloaderConfigDto, DownloaderViewModel>()
                .ReverseMap();
            cfg.CreateMap<GameLinkDto, GameLinkViewModel>().ReverseMap();
            cfg.CreateMap<GameWithStatsDto, GameWithStatsViewModel>().ConstructUsing(dto =>
                    new GameWithStatsViewModel(Ioc.Default.GetService<GameWithStatsService>())
                    {
                        AreCommandsVisible = false
                    }
                )
                .ForMember(vm => vm.AreCommandsVisible, exp => exp.Ignore());

            cfg.AddGlobalIgnore("InitCmd");
        });

        return mapperConfiguration;
    }
}