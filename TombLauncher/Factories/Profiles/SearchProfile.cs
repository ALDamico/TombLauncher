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
            .ForMember(dto => dto.TitlePic, opt => opt.MapFrom(vm => ImageUtils.ToByteArray(vm.TitlePic)))
            .ForMember(dto => dto.SourceSiteDisplayName, opt => opt.Ignore());
        CreateMap<IMultiSourceSearchResultMetadata, MultiSourceGameSearchResultMetadataViewModel>()
            .ConstructUsing((src, ctx) =>
                new MultiSourceGameSearchResultMetadataViewModel((GameSearchResultService)serviceFactory(typeof(GameSearchResultService))))
            .ForMember(vm => vm.InstallProgress, m => m.Ignore())
            .ForMember(vm => vm.InstalledGame, m => m.Ignore())
            .ForMember(vm => vm.IsNewlyAdded, m => m.Ignore())
            .ForMember(vm => vm.IsRecentlyUpdated, m => m.Ignore());
        CreateMap<MultiSourceGameSearchResultMetadataViewModel, IMultiSourceSearchResultMetadata>()
            .ConstructUsing(vm => new MultiSourceSearchResultMetadataDto());
        CreateMap<MultiSourceGameSearchResultMetadataViewModel, GameSearchResultMetadataDto>();

        CreateMap<DownloaderSearchPayload, DownloaderSearchPayloadViewModel>()
            .ForMember(m => m.AvailableDifficulties, opt => opt.Ignore())
            .ForMember(m => m.AvailableEngines, opt => opt.Ignore())
            .ReverseMap();
    }
}