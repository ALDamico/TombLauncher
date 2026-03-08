using AutoMapper;
using TombLauncher.Contracts.Enums;
using TombLauncher.Core.Dtos;
using TombLauncher.ViewModels;

namespace TombLauncher.Factories.Profiles;

internal class StatisticsProfile : Profile
{
    public StatisticsProfile()
    {
        CreateMap<DayOfWeekStatisticsDto, DayOfWeekAverageTimeStatisticsViewModel>()
            .ForMember(vm => vm.MetaData, opt => opt.Ignore())
            .ForMember(vm => vm.Coordinate, opt => opt.Ignore())
            .ForMember(vm => vm.Index, opt => opt.Ignore());
        CreateMap<DailyStatisticsDto, DailyStatisticsViewModel>()
            .ForMember(vm => vm.MetaData, opt => opt.Ignore())
            .ForMember(vm => vm.Coordinate, opt => opt.Ignore());
        CreateMap<GameStatisticsDto, GameStatisticsViewModel>();


        CreateMap<SavegameViewModel, FileBackupDto>()
            .ConstructUsingServiceLocator()
            .ForMember(m => m.FileType,
                opt => opt.MapFrom(vm => vm.IsStartOfLevel ? FileType.SavegameStartOfLevel : FileType.Savegame))
            .ForMember(m => m.Data, opt => opt.Ignore())
            .ForMember(m => m.GameId, opt => opt.Ignore())
            .ForMember(m => m.Md5, opt => opt.Ignore())
            .ForMember(m => m.Arguments, opt => opt.Ignore());

        CreateMap<FileBackupDto, SavegameViewModel>()
            .ForMember(m => m.IsStartOfLevel, opt => opt.MapFrom(dto => dto.FileType == FileType.SavegameStartOfLevel))
            .ForMember(m => m.SaveNumber, opt => opt.Ignore())
            .ForMember(m => m.SlotNumber, opt => opt.Ignore())
            .ForMember(m => m.LevelName, opt => opt.Ignore())
            .ForMember(m => m.UpdateStartOfLevelStateCmd, opt => opt.Ignore())
            .ForMember(m => m.DeleteSavegameCmd, opt => opt.Ignore())
            .ForMember(m => m.RestoreSavegameCmd, opt => opt.Ignore())
            .ForMember(m => m.Length, opt => opt.MapFrom(vm => vm.Data.Length));
    }
}