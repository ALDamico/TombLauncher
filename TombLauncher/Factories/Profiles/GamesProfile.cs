using AutoMapper;
using TombLauncher.Core.Dtos;
using TombLauncher.Data.Models;
using TombLauncher.Factories.Mapping;
using TombLauncher.Utils;
using TombLauncher.ViewModels;

namespace TombLauncher.Factories.Profiles;

internal class GamesProfile : Profile
{
    public GamesProfile()
    {
        CreateMap<GameHashes, GameHashDto>().ReverseMap();
        CreateMap<GameLink, GameLinkDto>().ReverseMap();
        CreateMap<Game, GameMetadataDto>()
            .ForMember(g => g.ExecutablePath, opt => opt.MapFrom<GameExecutableResolver>())
            .ForMember(g => g.SetupExecutable, opt => opt.MapFrom<SetupExecutableResolver>())
            .ForMember(g => g.SetupExecutableArgs, opt => opt.Ignore())
            .ForMember(g => g.CommunitySetupExecutable, opt => opt.MapFrom<CommunitySetupExecutableResolver>())
            .ReverseMap();

        CreateMap<GameMetadataDto, GameMetadataViewModel>()
            .ForMember(dto => dto.TitlePic, opt => opt.MapFrom(dto => ImageUtils.ToBitmap(dto.TitlePic)));
        CreateMap<GameMetadataViewModel, GameMetadataDto>()
            .ForMember(dto => dto.TitlePic, opt => opt.MapFrom(vm => ImageUtils.ToByteArray(vm.TitlePic)));
        
        CreateMap<GameLinkDto, GameLinkViewModel>().ReverseMap();
        CreateMap<GameWithStatsDto, GameWithStatsViewModel>().ConstructUsing(dto =>
                new GameWithStatsViewModel()
                {
                    AreCommandsVisible = false
                }
            )
            .ForMember(vm => vm.AreCommandsVisible, exp => exp.Ignore());
    }
}