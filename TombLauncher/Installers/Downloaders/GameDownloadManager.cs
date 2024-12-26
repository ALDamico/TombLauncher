using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TombLauncher.Contracts.Downloaders;
using TombLauncher.Contracts.Dtos;
using TombLauncher.Contracts.Progress;
using TombLauncher.Core.Utils;

namespace TombLauncher.Installers.Downloaders;

public class GameDownloadManager : IGameDownloadManager
{
    public GameDownloadManager(CancellationTokenSource cancellationTokenSource, IGameMerger merger)
    {
        _cancellationTokenSource = cancellationTokenSource;
        _merger = merger;
        Downloaders = new List<IGameDownloader>();
    }
    public List<IGameDownloader> Downloaders { get; init; }
    private CancellationTokenSource _cancellationTokenSource;
    private readonly IGameMerger _merger;

    public async Task<List<IMultiSourceSearchResultMetadata>> GetGames(DownloaderSearchPayload searchPayload)
    {
        var outputList = new List<IMultiSourceSearchResultMetadata>();
        var tasks = new List<Task<List<IGameSearchResultMetadata>>>();
        foreach (var downloader in Downloaders)
        {
            var gamesByDownloader = downloader.GetGames(searchPayload, _cancellationTokenSource.Token);
            tasks.Add(gamesByDownloader);
        }

        await Task.WhenAll(tasks);
        foreach (var completedTask in tasks)
        {
            if (!completedTask.IsCompleted)
            {
                continue;
            }

            var fetchedResults = completedTask.Result;
            _merger.Merge(outputList, fetchedResults.Cast<IGameSearchResultMetadata>().ToList());
        }

        return outputList;
    }

    public async Task<List<IGameSearchResultMetadata>> FetchNextPage()
    {
        var outputList = new List<IGameSearchResultMetadata>();
        var tasks = new List<Task<List<IGameSearchResultMetadata>>>();
        foreach (var downloader in Downloaders.Where(d => d.HasMorePages()))
        {
            var gamesByDownloader = downloader.FetchNextPage(_cancellationTokenSource.Token);
            tasks.Add(gamesByDownloader);
            // TODO Merge games somehow
            //outputList.AddRange(gamesByDownloader);
        }

        await Task.WhenAll(tasks);
        foreach (var completedTask in tasks)
        {
            if (!completedTask.IsCompleted)
            {
                continue;
            }
            
            outputList.AddRange(completedTask.Result);
        }

        return outputList;
    }

    public async Task<GameMetadataDto> FetchDetails(IGameSearchResultMetadata game)
    {
        var downloader = Downloaders.FirstOrDefault(d => d.BaseUrl == game.BaseUrl);
        if (downloader != null)
        {
            return await downloader.FetchDetails(game, _cancellationTokenSource.Token);
        }

        return null;
    }

    public async Task<IMultiSourceSearchResultMetadata> FetchAllDetails(IMultiSourceSearchResultMetadata game)
    {
        var searchPayload = new DownloaderSearchPayload()
        {
            LevelName = game.Title
        };
        var tasks = new List<Task<List<IGameSearchResultMetadata>>>();
        foreach (var downloader in Downloaders)
        {
            var searchResult = downloader.GetGames(searchPayload, CancellationToken.None);
            tasks.Add(searchResult);
        }

        await Task.WhenAll(tasks);
        var allResults = tasks.SelectMany(t => t.Result).ToList();

        var gameClone = new MultiSourceSearchResultMetadataDto()
        {
            Author = game.Author,
            Difficulty = game.Difficulty,
            Engine = game.Engine,
            Length = game.Length,
            Rating = game.Rating,
            Setting = game.Setting,
            Sources = new HashSet<IGameSearchResultMetadata>(),
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
        
        Merge(new List<IMultiSourceSearchResultMetadata>(){gameClone}, allResults);

        return gameClone;
    }

    public void CancelCurrentAction()
    {
        _cancellationTokenSource.Cancel();
        _cancellationTokenSource = new CancellationTokenSource();
    }

    public bool HasMoreResults()
    {
        return Downloaders.Any(d => d.HasMorePages());
    }

    public void Merge(ICollection<IMultiSourceSearchResultMetadata> fullList,
        ICollection<IGameSearchResultMetadata> newElements)
    {
        _merger.Merge(fullList, newElements);
    }

    public async Task<string> DownloadGame(IGameSearchResultMetadata metadata, IProgress<DownloadProgressInfo> downloadProgress, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var baseUrl = metadata.BaseUrl;
        var downloader = Downloaders.FirstOrDefault(d => d.BaseUrl == baseUrl);
        var downloadPath = PathUtils.GetRandomTempDirectory();
        var tempZipName = Path.GetRandomFileName();
        var fullFilePath = Path.Combine(downloadPath, tempZipName);
        await using var file = new FileStream(fullFilePath, FileMode.Create);
        await downloader.DownloadGame(metadata, file, downloadProgress, cancellationToken);
        return fullFilePath;

    }
}