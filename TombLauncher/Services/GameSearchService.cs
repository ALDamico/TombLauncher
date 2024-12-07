using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.DependencyInjection;
using JamSoft.AvaloniaUI.Dialogs;
using JamSoft.AvaloniaUI.Dialogs.MsgBox;
using TombLauncher.Data.Database.UnitOfWork;
using TombLauncher.Data.Dto;
using TombLauncher.Data.Models;
using TombLauncher.Extensions;
using TombLauncher.Installers;
using TombLauncher.Installers.Downloaders;
using TombLauncher.Localization;
using TombLauncher.Navigation;
using TombLauncher.Progress;
using TombLauncher.ViewModels;
using TombLauncher.ViewModels.Pages;

namespace TombLauncher.Services;

public class GameSearchService : IViewService
{
    public GameSearchService(GameDownloadManager gameDownloadManager, GamesUnitOfWork gamesUnitOfWork, TombRaiderLevelInstaller levelInstaller,
        LocalizationManager localizationManager, TombRaiderEngineDetector engineDetector, NavigationManager navigationManager,
        IMessageBoxService messageBoxService, IDialogService dialogService)
    {
        GameDownloadManager = gameDownloadManager;
        GamesUnitOfWork = gamesUnitOfWork;
        LevelInstaller = levelInstaller;
        LocalizationManager = localizationManager;
        EngineDetector = engineDetector;
        NavigationManager = navigationManager;
        MessageBoxService = messageBoxService;
        DialogService = dialogService;
    }
    public GameDownloadManager GameDownloadManager { get; }
    public GamesUnitOfWork GamesUnitOfWork { get; }
    public TombRaiderLevelInstaller LevelInstaller { get; }
    public TombRaiderEngineDetector EngineDetector { get; }
    public LocalizationManager LocalizationManager { get; }
    public NavigationManager NavigationManager { get; }
    public IMessageBoxService MessageBoxService { get; }
    public IDialogService DialogService { get; }

    public bool CanInstall(MultiSourceGameSearchResultMetadataViewModel obj)
    {
        if (obj == null) return false;
        var links = obj.Sources.Select(s => s.DownloadLink).ToList();
        var gameDto = GamesUnitOfWork.GetGameByLinks(LinkType.Download, links);
        return gameDto == null;
    }

    public async Task Install(MultiSourceGameSearchResultMetadataViewModel gameToInstall)
    {
        var downloadPath = await GameDownloadManager.DownloadGame(gameToInstall, new Progress<DownloadProgressInfo>(
            p =>
            {
                gameToInstall.TotalBytes = p.TotalBytes;
                gameToInstall.CurrentBytes = p.BytesDownloaded;
                gameToInstall.DownloadSpeed = p.DownloadSpeed;
            }));
        Dispatcher.UIThread.Invoke(() =>
        {
            gameToInstall.TotalBytes = 0;
            gameToInstall.DownloadSpeed = 0;
        });
        var hashCalculator = Ioc.Default.GetRequiredService<GameFileHashCalculator>();
        var hashes = await hashCalculator.CalculateHashes(downloadPath);
        if (GamesUnitOfWork.ExistsHashes(hashes))
        {
            var result = await MessageBoxService.Show("Game already installed",
                "This game is already installed. Do you want to install anyway?", MsgBoxButton.YesNo,
                MsgBoxImage.Question);
            if (result.ButtonResult == MsgBoxButtonResult.No)
            {
                return;
            }
        }

        //var engine = _engineDetector.Detect(downloadPath);
        var dto = await GameDownloadManager.FetchDetails(gameToInstall);
        dto.Guid = Guid.NewGuid();
        var installLocation = await LevelInstaller.Install(downloadPath, dto, new Progress<CopyProgressInfo>(a =>
        {
            gameToInstall.TotalBytes = 100;
            gameToInstall.CurrentBytes = a.Percentage.GetValueOrDefault();
        }));
        dto.InstallDate = DateTime.Now;
        dto.InstallDirectory = installLocation;
        var exePath = EngineDetector.GetGameExecutablePath(installLocation);
        dto.ExecutablePath = exePath;
        GamesUnitOfWork.UpsertGame(dto);
        hashes.ForEach(h => h.GameId = dto.Id);
        GamesUnitOfWork.SaveHashes(hashes);
        GamesUnitOfWork.SaveLink(new GameLinkDto()
            { Link = gameToInstall.DownloadLink, LinkType = LinkType.Download, GameId = dto.Id });
        GamesUnitOfWork.SaveLink(new GameLinkDto()
            { Link = gameToInstall.DetailsLink, LinkType = LinkType.Details, GameId = dto.Id });
        if (gameToInstall.HasReviews)
        {
            GamesUnitOfWork.SaveLink(new GameLinkDto()
                { Link = gameToInstall.ReviewsLink, LinkType = LinkType.Reviews, GameId = dto.Id });
        }

        if (gameToInstall.HasWalkthrough)
        {
            GamesUnitOfWork.SaveLink(new GameLinkDto()
                { Link = gameToInstall.WalkthroughLink, LinkType = LinkType.Walkthrough, GameId = dto.Id });
        }

        GamesUnitOfWork.Save();
    }

    public async Task LoadMore(GameSearchViewModel target)
    {
        target.IsBusy = true;
        target.BusyMessage = "Caricamento in corso...";
        var nextPage = await GameDownloadManager.FetchNextPage();
        GameDownloadManager.Merge(target.FetchedResults, nextPage);
        var gamesByLinks =
            GamesUnitOfWork.GetGamesByLinksDictionary(LinkType.Download, nextPage.Select(p => p.DownloadLink).ToList());
        foreach (var game in Enumerable.Where(target.FetchedResults, r => r.InstalledGame == null))
        {
            if (gamesByLinks.TryGetValue(game.DownloadLink, out var dto))
            {
                game.InstalledGame = dto.ToViewModel();
            }
        }

        target.HasMoreResults = GameDownloadManager.HasMoreResults();
        target.IsBusy = false;
    }

    public bool CanLoadMore() => GameDownloadManager.HasMoreResults();

    public async Task Open(GameSearchViewModel target, IGameSearchResultMetadata gameToOpen)
    {
        target.IsBusy = true;
        
        var details = await GameDownloadManager.FetchDetails(gameToOpen);
        if (details != null)
        {
            var detailsViewModel = details.ToViewModel();
            if (gameToOpen.TitlePic == null || gameToOpen.TitlePic.Size == new Size(0, 0))
            {
                gameToOpen.TitlePic = detailsViewModel.TitlePic;
            }

            var gameDetailsService = Ioc.Default.GetRequiredService<GameDetailsService>();
            var gameWithStatsService = Ioc.Default.GetRequiredService<GameWithStatsService>();
            var vm = new GameDetailsViewModel(gameDetailsService,
                new GameWithStatsViewModel(gameWithStatsService) { GameMetadata = detailsViewModel });
            target.IsBusy = false;
            NavigationManager.NavigateTo(vm);
            return;
        }

        target.IsBusy = false;
    }

    public async Task Search(GameSearchViewModel target)
    {
        target.IsBusy = true;
        target.BusyMessage = "Avvio ricerca...";
        target.FetchedResults = new ObservableCollection<MultiSourceGameSearchResultMetadataViewModel>();
        try
        {
            var games = await GameDownloadManager.GetGames(target.SearchPayload.ToDto());
            var downloadLinks = games.SelectMany(g => g.Sources).Select(s => s.DownloadLink) /*TODO Remove this*/
                .Where(s => !string.IsNullOrWhiteSpace(s)).ToList();
            var installedGames = GamesUnitOfWork.GetGamesByLinksDictionary(LinkType.Download, downloadLinks);
            foreach (var game in games)
            {
                if (game.DownloadLink == null) continue;
                if (installedGames.TryGetValue(game.DownloadLink, out var installedGame))
                {
                    game.InstalledGame = installedGame.ToViewModel();
                }
            }

            Dispatcher.UIThread.Invoke(() => target.FetchedResults = games.ToObservableCollection());
            target.HasMoreResults = GameDownloadManager.HasMoreResults();
        }
        catch (OperationCanceledException)
        {
            target.FetchedResults = new ObservableCollection<MultiSourceGameSearchResultMetadataViewModel>();
        }

        target.IsBusy = false;
    }

    public void Cancel()
    {
        GameDownloadManager.CancelCurrentAction();
    }
}