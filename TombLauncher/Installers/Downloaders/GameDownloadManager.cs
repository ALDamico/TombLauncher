﻿using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.DependencyInjection;
using TombLauncher.Dto;
using TombLauncher.ViewModels;

namespace TombLauncher.Installers.Downloaders;

public class GameDownloadManager
{
    public GameDownloadManager(CancellationTokenSource cancellationTokenSource)
    {
        _cancellationTokenSource = cancellationTokenSource;
        Downloaders = new List<IGameDownloader>();
    }
    public List<IGameDownloader> Downloaders { get; }
    private CancellationTokenSource _cancellationTokenSource;

    public async Task<List<GameSearchResultMetadataViewModel>> GetGames(DownloaderSearchPayload searchPayload)
    {
        var outputList = new List<GameSearchResultMetadataViewModel>();
        foreach (var downloader in Downloaders)
        {
            var gamesByDownloader = await downloader.GetGames(searchPayload, _cancellationTokenSource.Token);
            // TODO Merge games somehow
            outputList.AddRange(gamesByDownloader);
        }

        return outputList;
    }

    public async Task<GameMetadataDto> FetchDetails(GameSearchResultMetadataViewModel game)
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
}