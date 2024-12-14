using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.DependencyInjection;
using JamSoft.AvaloniaUI.Dialogs;
using JamSoft.AvaloniaUI.Dialogs.MsgBox;
using TombLauncher.Contracts.Localization;
using TombLauncher.Data.Database.UnitOfWork;
using TombLauncher.Data.Dto;
using TombLauncher.Data.Models;
using TombLauncher.Extensions;
using TombLauncher.Installers.Downloaders;
using TombLauncher.Localization;
using TombLauncher.Navigation;
using TombLauncher.Progress;
using TombLauncher.ViewModels;
using TombLauncher.ViewModels.Pages;

namespace TombLauncher.Services;

public class GameSearchService : IViewService
{
    public GameSearchService(GameDownloadManager gameDownloadManager, GamesUnitOfWork gamesUnitOfWork,
        ILocalizationManager localizationManager, NavigationManager navigationManager,
        IMessageBoxService messageBoxService, IDialogService dialogService)
    {
        GameDownloadManager = gameDownloadManager;
        GamesUnitOfWork = gamesUnitOfWork;
        LocalizationManager = localizationManager;
        NavigationManager = navigationManager;
        MessageBoxService = messageBoxService;
        DialogService = dialogService;
    }
    public GameDownloadManager GameDownloadManager { get; }
    public GamesUnitOfWork GamesUnitOfWork { get; }
    public ILocalizationManager LocalizationManager { get; }
    public NavigationManager NavigationManager { get; }
    public IMessageBoxService MessageBoxService { get; }
    public IDialogService DialogService { get; }

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

        target.HasMoreResults = CanLoadMore();
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