using AutoMapper;
using Microsoft.CodeAnalysis.FlowAnalysis;
using TombLauncher.Core.Dtos;
using TombLauncher.Data.Models;
using TombLauncher.Factories.Mapping;
using TombLauncher.ViewModels.Dialogs;

namespace TombLauncher.Factories.Profiles;

public class LaunchOptionsProfile : Profile
{
    public LaunchOptionsProfile()
    {
        CreateMap<LaunchOptionsDialogViewModel, LaunchOptionsDto>()
            .ForMember(dto => dto.GameEngine, opt => opt.MapFrom(vm => vm.SelectedEngine))
            .ForMember(dto => dto.GameExecutable, opt => opt.MapFrom(MappingUtils.MapGameExecutable))
            .ForMember(dto => dto.SetupExecutable, opt => opt.MapFrom(MappingUtils.MapSetupExecutable))
            .ForMember(dto => dto.CommunitySetupExecutable, opt => opt.MapFrom(MappingUtils.MapCommunitySetupExecutable))
            .ForMember(dto => dto.GameId, opt => opt.MapFrom(vm => vm.TargetGame.Id));
        
       /* CreateMap<Game, LaunchOptionsDto>()
            .ForMember(dto => dto.GameEngine, opt => opt.MapFrom(g => g.GameEngine))
            .ForMember(dto => dto.GameId, opt => opt.MapFrom(g => g.Id))
            .ForMember(dto => dto.GameExecutable, opt => opt.MapFrom(g => new FileBackupDto(){FileName = g.FileBackups.whe}))*/
    }
}