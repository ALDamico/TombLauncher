using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TombLauncher.Data.Dto;
using TombLauncher.Progress;
using TombLauncher.Utils;
using TombLauncher.ViewModels;

namespace TombLauncher.Installers.Downloaders;

public class GameDownloadManager
{
    public GameDownloadManager(CancellationTokenSource cancellationTokenSource, IGameMerger merger)
    {
        _cancellationTokenSource = cancellationTokenSource;
        _merger = merger;
        Downloaders = new List<IGameDownloader>();
    }
    public List<IGameDownloader> Downloaders { get; }
    private CancellationTokenSource _cancellationTokenSource;
    private readonly IGameMerger _merger;

    public async Task<List<MultiSourceGameSearchResultMetadataViewModel>> GetGames(DownloaderSearchPayload searchPayload)
    {
        var outputList = new List<MultiSourceGameSearchResultMetadataViewModel>();
        var tasks = new List<Task<List<GameSearchResultMetadataViewModel>>>();
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
        var tasks = new List<Task<List<GameSearchResultMetadataViewModel>>>();
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

    public void CancelCurrentAction()
    {
        _cancellationTokenSource.Cancel();
        _cancellationTokenSource = new CancellationTokenSource();
    }

    public bool HasMoreResults()
    {
        return Downloaders.Any(d => d.HasMorePages());
    }

    public void Merge(ICollection<MultiSourceGameSearchResultMetadataViewModel> fullList,
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