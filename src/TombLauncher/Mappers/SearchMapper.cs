using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using TombLauncher.Contracts.Downloaders;
using TombLauncher.Core.Dtos;
using TombLauncher.Core.Extensions;
using TombLauncher.Services;
using TombLauncher.ViewModels;

namespace TombLauncher.Mappers;

public class SearchMapper
{
    public GameSearchResultMetadataDto ToDto(IGameSearchResultMetadata vm)
    {
        return new GameSearchResultMetadataDto()
        {
            Author = vm.Author,
            AuthorFullName = vm.AuthorFullName,
            BaseUrl = vm.BaseUrl,
            Description = vm.Description,
            DetailsLink = vm.DetailsLink,
            Difficulty = vm.Difficulty,
            DownloadLink = vm.DownloadLink,
            Engine = vm.Engine,
            Length = vm.Length,
            Rating = vm.Rating,
            ReleaseDate = vm.ReleaseDate,
            ReviewsLink = vm.ReviewsLink,
            Setting = vm.Setting,
            SizeInMb = vm.SizeInMb,
            Title = vm.Title,
            TitlePic = vm.TitlePic,
            ReviewCount = vm.ReviewCount,
            WalkthroughLink = vm.WalkthroughLink,
        };
    }

    public IEnumerable<GameSearchResultMetadataDto> ToDtos(IEnumerable<IGameSearchResultMetadata> vm) =>
        vm.Select(ToDto);

    public IGameSearchResultMetadata ToDto(MultiSourceGameSearchResultMetadataViewModel vm)
    {
        return new GameSearchResultMetadataDto()
        {
            Author = vm.Author,
            AuthorFullName = vm.AuthorFullName,
            BaseUrl = vm.BaseUrl,
            Description = vm.Description,
            DetailsLink = vm.DetailsLink,
            Difficulty = vm.Difficulty,
            DownloadLink = vm.DownloadLink,
            Engine = vm.Engine,
            Length = vm.Length,
            Rating = vm.Rating,
            ReleaseDate = vm.ReleaseDate,
            ReviewsLink = vm.ReviewsLink,
            Setting = vm.Setting,
            SizeInMb = vm.SizeInMb,
            Title = vm.Title,
            TitlePic = vm.TitlePic,
            ReviewCount = vm.ReviewCount,
            WalkthroughLink = vm.WalkthroughLink
        };
    }

    public MultiSourceGameSearchResultMetadataViewModel ToViewModel(IMergedGameSearchResultMetadata metadata,
        GameSearchResultService gameSearchResultService)
    {
        return new MultiSourceGameSearchResultMetadataViewModel(gameSearchResultService)
        {
            TitlePic = metadata.TitlePic ?? "",
            Author = metadata.Author ?? "",
            AuthorFullName = metadata.AuthorFullName ?? "",
            BaseUrl = metadata.BaseUrl,
            Description = metadata.Description ?? "",
            DetailsLink = metadata.DetailsLink ?? "",
            Difficulty = metadata.Difficulty,
            DownloadLink = metadata.DownloadLink ?? "",
            Engine = metadata.Engine,
            Length = metadata.Length,
            Rating = metadata.Rating,
            ReleaseDate = metadata.ReleaseDate,
            ReviewsLink = metadata.ReviewsLink ?? "",
            Setting = metadata.Setting ?? "",
            SizeInMb = metadata.SizeInMb,
            Sources = metadata.Sources.ToObservableCollection(),
            Title = metadata.Title,
            WalkthroughLink = metadata.WalkthroughLink ?? "",
            SourceSiteDisplayName = metadata.SourceSiteDisplayName,
        };
    }

    public IEnumerable<MultiSourceGameSearchResultMetadataViewModel> ToViewModels(
        IEnumerable<IMergedGameSearchResultMetadata> metadata, GameSearchResultService gameSearchResultService) =>
        metadata.Select(m => ToViewModel(m, gameSearchResultService));

    public ObservableCollection<MultiSourceGameSearchResultMetadataViewModel> ToObservableCollection(
        IEnumerable<IMergedGameSearchResultMetadata> metadata, GameSearchResultService gameSearchResultService) =>
        ToViewModels(metadata, gameSearchResultService).ToObservableCollection();

    public IMergedGameSearchResultMetadata ToMergedDto(MultiSourceGameSearchResultMetadataViewModel vm)
    {
        return new MergedGameSearchResultDto()
        {
            TitlePic = vm.TitlePic,
            Author = vm.Author,
            AuthorFullName = vm.AuthorFullName,
            BaseUrl = vm.BaseUrl,
            Description = vm.Description,
            DetailsLink = vm.DetailsLink,
            Difficulty = vm.Difficulty,
            DownloadLink = vm.DownloadLink,
            Engine = vm.Engine,
            Length = vm.Length,
            Rating = vm.Rating,
            ReleaseDate = vm.ReleaseDate,
            ReviewsLink = vm.ReviewsLink,
            Setting = vm.Setting,
            SizeInMb = vm.SizeInMb,
            Sources = vm.Sources.ToHashSet(),
            Title = vm.Title,
            WalkthroughLink = vm.WalkthroughLink,
            SourceSiteDisplayName = vm.SourceSiteDisplayName
        };
    }

    public List<IMergedGameSearchResultMetadata> ToMergedDtos(IEnumerable<MultiSourceGameSearchResultMetadataViewModel> targetFetchedResults)
    {
        return targetFetchedResults.Select(ToMergedDto).ToList();
    }
}