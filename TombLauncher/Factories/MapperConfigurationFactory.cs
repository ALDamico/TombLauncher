using System;
using AutoMapper;
using Newtonsoft.Json;
using TombLauncher.Contracts.Downloaders;
using TombLauncher.Contracts.Enums;
using TombLauncher.Contracts.Localization.Dtos;
using TombLauncher.Core.Dtos;
using TombLauncher.Core.Savegames;
using TombLauncher.Core.Utils;
using TombLauncher.Data.Models;
using TombLauncher.Factories.Mapping;
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
                    new MultiSourceGameSearchResultMetadataViewModel())
                .ForMember(vm => vm.InstallProgress, m => m.Ignore())
                .ForMember(vm => vm.InstalledGame, m => m.Ignore())
                ;
            cfg.CreateMap<MultiSourceGameSearchResultMetadataViewModel, IMultiSourceSearchResultMetadata>()
                .ConstructUsing(vm => new MultiSourceSearchResultMetadataDto());
            cfg.CreateMap<MultiSourceGameSearchResultMetadataViewModel, GameSearchResultMetadataDto>();
            cfg.CreateMap<DownloaderConfiguration, DownloaderViewModel>()
                .ReverseMap();
            cfg.CreateMap<GameLinkDto, GameLinkViewModel>().ReverseMap();
            cfg.CreateMap<GameWithStatsDto, GameWithStatsViewModel>().ConstructUsing(dto =>
                    new GameWithStatsViewModel()
                    {
                        AreCommandsVisible = false
                    }
                )
                .ForMember(vm => vm.AreCommandsVisible, exp => exp.Ignore());

            cfg.CreateMap<DayOfWeekStatisticsDto, DayOfWeekAverageTimeStatisticsViewModel>();
            cfg.CreateMap<DailyStatisticsDto, DailyStatisticsViewModel>();
            cfg.CreateMap<GameStatisticsDto, GameStatisticsViewModel>();
            cfg.CreateMap<FileBackup, FileBackupDto>().ReverseMap();
            cfg.CreateMap<SavegameViewModel, FileBackupDto>()
                .ConstructUsingServiceLocator()
                .ForMember(m => m.FileType,
                    opt => opt.MapFrom(vm => vm.IsStartOfLevel ? FileType.SavegameStartOfLevel : FileType.Savegame));
            
            cfg.CreateMap<FileBackupDto, SavegameViewModel>()
                .ForMember(m => m.IsStartOfLevel, opt => opt.MapFrom(dto => dto.FileType == FileType.SavegameStartOfLevel))
                .ForMember(m => m.SaveNumber, opt => opt.Ignore())
                .ForMember(m => m.SlotNumber, opt => opt.Ignore())
                .ForMember(m => m.LevelName, opt => opt.Ignore())
                .ForMember(m => m.UpdateStartOfLevelStateCmd, opt => opt.Ignore());

            var mappingHeaderReader = new SavegameHeaderReader();
            var headerProxy = new SavegameHeaderProxy(mappingHeaderReader);

            var levelNameResolver = new LevelNameResolver(headerProxy);
            var slotNumberResolver = new SlotNumberResolver(headerProxy);
            var saveNumberResolver = new SaveNumberResolver(headerProxy);

            cfg.CreateMap<FileBackupDto, SavegameBackupDto>()
                .ForMember(m => m.Md5, opt => opt.MapFrom(file => Md5Utils.ComputeMd5Hash(file.Data)))
                .ForMember(m => m.LevelName, opt => opt.MapFrom(levelNameResolver))
                .ForMember(m => m.SlotNumber, opt => opt.MapFrom(slotNumberResolver))
                .ForMember(m => m.SaveNumber, opt => opt.MapFrom(saveNumberResolver));

            cfg.CreateMap<FileBackup, SavegameBackupDto>();
                cfg.CreateMap<SavegameBackupDto, FileBackup>()
                    .ConstructUsing(dto => new FileBackup()
                    {
                        SavegameMetadata = new SavegameMetadata()
                    })
                    .ForPath(dto => dto.SavegameMetadata.Id, opt => opt.MapFrom(o => o.MetadataId))
                    .ForPath(dto => dto.SavegameMetadata.LevelName, opt => opt.MapFrom(o => o.LevelName))
                    .ForPath(dto => dto.SavegameMetadata.SaveNumber, opt => opt.MapFrom(o => o.SaveNumber))
                    .ForPath(dto => dto.SavegameMetadata.SlotNumber, opt => opt.MapFrom(o => o.SlotNumber))
                    .ForPath(dto => dto.SavegameMetadata.FileBackupId, opt => opt.MapFrom(o => o.Id)); 
                
//                .ReverseMap();
            cfg.CreateMap<SavegameMetadata, SavegameBackupDto>()
                .ForMember(dto => dto.Id, opt => opt.MapFrom(savegame => savegame.FileBackup.Id))
                .ForMember(dto => dto.MetadataId, opt => opt.MapFrom(savegame => savegame.Id))
                .ReverseMap();

            cfg.AddGlobalIgnore("InitCmd");
        });

        return mapperConfiguration;
    }
}