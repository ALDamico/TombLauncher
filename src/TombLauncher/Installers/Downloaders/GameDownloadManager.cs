using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TombLauncher.Contracts.Downloaders;
using TombLauncher.Contracts.Progress;
using TombLauncher.Core.Dtos;
using TombLauncher.Core.Utils;

namespace TombLauncher.Installers.Downloaders;

public class GameDownloadManager
{
    public GameDownloadManager(IGameMerger merger)
    {
        _merger = merger;
        Downloaders = [];
    }

    public List<IGameDownloader> Downloaders { get; init; }
    private CancellationTokenSource _cancellationTokenSource = new();
    private readonly IGameMerger _merger;

    public async Task<(List<IMergedGameSearchResultMetadata> Results, int? MaxTotalPages)> GetGames(
        IReadOnlyList<IGameDownloader> downloaders,
        DownloaderSearchPayload searchPayload, int page)
    {
        var outputList = new List<IMergedGameSearchResultMetadata>();
        var tasks = downloaders
            .Select(d => d.Search.GetGames(searchPayload, page, _cancellationTokenSource.Token))
            .ToList();

        await Task.WhenAll(tasks);

        var maxTotalPages = 0;
        foreach (var completedTask in tasks.Where(t => t.IsCompleted))
        {
            var resultPage = completedTask.Result;
            _merger.Merge(outputList, resultPage.Results.ToList());
            if (resultPage.TotalPages.HasValue && resultPage.TotalPages.Value > maxTotalPages)
                maxTotalPages = resultPage.TotalPages.Value;
        }

        return (outputList, maxTotalPages > 0 ? maxTotalPages : null);
    }

    public async Task<IGameMetadata?> FetchDetails(IGameSearchResultMetadata game)
    {
        var downloader = Downloaders.FirstOrDefault(d => d.BaseUrl == game.BaseUrl);
        if (downloader != null)
            return await downloader.Details.FetchDetails(game, _cancellationTokenSource.Token);

        return null;
    }

    public async Task<IMergedGameSearchResultMetadata> FetchAllDetails(
        IReadOnlyList<IGameDownloader> downloaders,
        IMergedGameSearchResultMetadata game)
    {
        var searchPayload = new DownloaderSearchPayload() { LevelName = game.Title };
        var tasks = downloaders
            .Select(d => d.Search.GetGames(searchPayload, 1, _cancellationTokenSource.Token))
            .ToList();

        await Task.WhenAll(tasks);
        var allResults = tasks.SelectMany(t => t.Result.Results).ToList();

        var gameClone = new MergedGameSearchResultDto()
        {
            Author = game.Author,
            Difficulty = game.Difficulty,
            Engine = game.Engine,
            Length = game.Length,
            Rating = game.Rating,
            Setting = game.Setting,
            Sources = [],
            Title = game.Title,
            BaseUrl = game.BaseUrl,
            DetailsLink = game.DetailsLink,
            DownloadLink = game.DownloadLink,
            ReleaseDate = game.ReleaseDate,
            ReviewsLink = game.ReviewsLink,
            TitlePic = game.TitlePic,
            WalkthroughLink = game.WalkthroughLink,
            AuthorFullName = game.AuthorFullName,
            SizeInMb = game.SizeInMb,
            SourceSiteDisplayName = game.SourceSiteDisplayName
        };

        //gameClone.Sources.Add(game);

        Merge([gameClone], allResults);

        return gameClone;
    }

    public void CancelCurrentAction()
    {
        _cancellationTokenSource.Cancel();
        _cancellationTokenSource.Dispose();
        _cancellationTokenSource = new CancellationTokenSource();
    }

    public void Merge(ICollection<IMergedGameSearchResultMetadata> fullList,
        ICollection<IGameSearchResultMetadata> newElements)
    {
        _merger.Merge(fullList, newElements);
    }

    public async Task<string> DownloadGame(IGameSearchResultMetadata metadata,
        IProgress<DownloadProgressInfo> downloadProgress, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var baseUrl = metadata.BaseUrl;
        var downloader = Downloaders.FirstOrDefault(d => d.BaseUrl == baseUrl);
        if (downloader == null) throw new InvalidOperationException($"Downloader for {baseUrl} not found.");

        var downloadPath = PathUtils.GetRandomTempDirectory();
        var tempZipName = Path.GetRandomFileName();
        var fullFilePath = Path.Combine(downloadPath, tempZipName);
        await using var file = new FileStream(fullFilePath, FileMode.Create);
        await downloader.Installer.DownloadGame(metadata, file, downloadProgress, cancellationToken);
        return fullFilePath;
    }
}