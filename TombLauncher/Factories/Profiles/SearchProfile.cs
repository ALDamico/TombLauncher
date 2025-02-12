using AutoMapper;
using TombLauncher.Contracts.Downloaders;
using TombLauncher.Core.Dtos;
using TombLauncher.Utils;
using TombLauncher.ViewModels;

namespace TombLauncher.Factories.Profiles;

public class SearchProfile : Profile
{
    public SearchProfile()
    {
        CreateMap<IGameSearchResultMetadata, GameSearchResultMetadataViewModel>();
        CreateMap<GameSearchResultMetadataViewModel, IGameSearchResultMetadata>()
            .ConstructUsing(vm => new GameSearchResultMetadataDto())
            .ForMember(dto => dto.TitlePic, opt => opt.MapFrom(vm => ImageUtils.ToByteArray(vm.TitlePic)))
            .ForMember(dto => dto.SourceSiteDisplayName, opt => opt.Ignore());
        CreateMap<IMultiSourceSearchResultMetadata, MultiSourceGameSearchResultMetadataViewModel>()
            .ConstructUsing(vm =>
                new MultiSourceGameSearchResultMetadataViewModel())
            .ForMember(vm => vm.InstallProgress, m => m.Ignore())
            .ForMember(vm => vm.InstalledGame, m => m.Ignore());
        CreateMap<MultiSourceGameSearchResultMetadataViewModel, IMultiSourceSearchResultMetadata>()
            .ConstructUsing(vm => new MultiSourceSearchResultMetadataDto());
        CreateMap<MultiSourceGameSearchResultMetadataViewModel, GameSearchResultMetadataDto>();
    }
}