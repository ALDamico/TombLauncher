using System;
using AutoMapper;
using TombLauncher.Contracts.Downloaders;
using TombLauncher.Core.Dtos;
using TombLauncher.Services;
using TombLauncher.Utils;
using TombLauncher.ViewModels;

namespace TombLauncher.Factories.Profiles;

public class SearchProfile : Profile
{
    public SearchProfile(Func<Type, object> serviceFactory)
    {
        CreateMap<IGameSearchResultMetadata, GameSearchResultMetadataViewModel>();
        CreateMap<GameSearchResultMetadataViewModel, IGameSearchResultMetadata>()
            .ConstructUsing(vm => new GameSearchResultMetadataDto())
            .ForMember(dto => dto.TitlePic, opt => opt.MapFrom(vm => vm.TitlePic != null ? ImageUtils.ToByteArray(vm.TitlePic) : null))
            .ForMember(dto => dto.SourceSiteDisplayName, opt => opt.Ignore());
        CreateMap<IMergedGameSearchResultMetadata, MultiSourceGameSearchResultMetadataViewModel>()
            .ConstructUsing((src, ctx) =>
                new MultiSourceGameSearchResultMetadataViewModel((GameSearchResultService)serviceFactory(typeof(GameSearchResultService))))
            .ForMember(vm => vm.InstallProgress, m => m.Ignore())
            .ForMember(vm => vm.InstalledGame, m => m.Ignore())
            .ForMember(vm => vm.IsNewlyAdded, m => m.Ignore())
            .ForMember(vm => vm.IsRecentlyUpdated, m => m.Ignore());
        CreateMap<MultiSourceGameSearchResultMetadataViewModel, IMergedGameSearchResultMetadata>()
            .ConstructUsing(vm => new MergedGameSearchResultDto());
        CreateMap<MultiSourceGameSearchResultMetadataViewModel, GameSearchResultMetadataDto>();

        CreateMap<DownloaderSearchPayload, DownloaderSearchPayloadViewModel>()
            .ForMember(m => m.AvailableDifficulties, opt => opt.Ignore())
            .ForMember(m => m.AvailableEngines, opt => opt.Ignore())
            .ReverseMap();
    }
}