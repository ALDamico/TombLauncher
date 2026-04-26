using System;
using AutoMapper;
using TombLauncher.Core.Dtos;
using TombLauncher.Data.Models;
using TombLauncher.Factories.Mapping;
using TombLauncher.Services;
using TombLauncher.Utils;
using TombLauncher.ViewModels;

namespace TombLauncher.Factories.Profiles;

internal class GamesProfile : Profile
{
    public GamesProfile(Func<Type, object> serviceFactory)
    {
        CreateMap<GameHashes, GameHashDto>().ReverseMap();
        CreateMap<Game, GameMetadataDto>()
            .ForMember(g => g.ExecutablePath, opt => opt.MapFrom<GameExecutableResolver>())
            .ForMember(g => g.SetupExecutable, opt => opt.MapFrom<SetupExecutableResolver>())
            .ForMember(g => g.SetupExecutableArgs, opt => opt.Ignore())
            .ForMember(g => g.CommunitySetupExecutable, opt => opt.MapFrom<CommunitySetupExecutableResolver>())
            .ForMember(g => g.InstalledFromSiteDisplayName,
                opt => opt.MapFrom(g => g.InstalledFromLink != null ? g.InstalledFromLink.DisplayName : null));

        CreateMap<GameMetadataDto, Game>()
            .ForMember(g => g.FileBackups, opt => opt.MapFrom<GameFileBackupsResolver>())
            .ForMember(g => g.PlaySessions, opt => opt.Ignore())
            .ForMember(g => g.Hashes, opt => opt.Ignore())
            .ForMember(g => g.Links, opt => opt.Ignore())
            .ForMember(g => g.InstalledFromLink, opt => opt.Ignore());

        CreateMap<GameMetadataDto, GameMetadataViewModel>()
            .ForMember(dto => dto.TitlePic, opt => opt.MapFrom(dto => ImageUtils.ToBitmap(dto.TitlePic)));

        CreateMap<GameMetadataViewModel, GameMetadataDto>()
            .ForMember(dto => dto.TitlePic, opt => opt.MapFrom(vm => vm.TitlePic != null ? ImageUtils.ToByteArray(vm.TitlePic) : null));

        CreateMap<GameWithStatsDto, GameWithStatsViewModel>()
            .ConstructUsing((dto, ctx) =>
                new GameWithStatsViewModel((GameWithStatsService)serviceFactory(typeof(GameWithStatsService))))
            .ForMember(vm => vm.AreCommandsVisible, exp => exp.Ignore())
            .AfterMap((_, vm) => vm.AreCommandsVisible = false);
    }
}