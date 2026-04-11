using AutoMapper;
using TombLauncher.Core.Dtos;
using TombLauncher.Factories.Mapping;
using TombLauncher.ViewModels.Pages;

namespace TombLauncher.Factories.Profiles;

public class LaunchOptionsProfile : Profile
{
    public LaunchOptionsProfile()
    {
        CreateMap<LaunchOptionsViewModel, LaunchOptionsDto>()
            .ForMember(dto => dto.GameEngine, opt => opt.MapFrom(vm => vm.SelectedEngine))
            .ForMember(dto => dto.GameExecutable, opt => opt.MapFrom(MappingUtils.MapGameExecutable))
            .ForMember(dto => dto.SetupExecutable, opt => opt.MapFrom(MappingUtils.MapSetupExecutable))
            .ForMember(dto => dto.CommunitySetupExecutable, opt => opt.MapFrom(MappingUtils.MapCommunitySetupExecutable))
            .ForMember(dto => dto.GameId, opt => opt.MapFrom(vm => vm.GameId))
            .ForMember(dto => dto.CompatibilityPrefixPath, opt => opt.MapFrom(vm => vm.CompatibilityPrefixPath));
    }
}