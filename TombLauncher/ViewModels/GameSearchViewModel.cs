using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
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

namespace TombLauncher.ViewModels;

public partial class GameSearchViewModel : PageViewModel
{
    public GameSearchViewModel(LocalizationManager localizationManager, GameDownloadManager gameDownloadManager,
        NavigationManager navigationManager,
        GamesUnitOfWork gamesUoW, IMessageBoxService messageBoxService, TombRaiderLevelInstaller levelInstaller,
        TombRaiderEngineDetector engineDetector) : base(localizationManager, messageBoxService)
    {
        _searchPayload = new DownloaderSearchPayloadViewModel();
        _navigationManager = navigationManager;
        _gameDownloadManager = gameDownloadManager;
        _gamesUoW = gamesUoW;
        _levelInstaller = levelInstaller;
        _engineDetector = engineDetector;
        SearchCmd = new RelayCommand(Search);
        OpenCmd = new RelayCommand<IGameSearchResultMetadata>(Open);
        LoadMoreCmd = new RelayCommand(LoadMore, CanLoadMore);
        IsCancelable = true;
        InstallCmd = new AsyncRelayCommand<MultiSourceGameSearchResultMetadataViewModel>(Install, CanInstall);
    }

    private bool CanInstall(MultiSourceGameSearchResultMetadataViewModel obj)
    {
        if (obj == null) return false;
        var links = obj.Sources.Select(s => s.DownloadLink).ToList();
        var gameDto = _gamesUoW.GetGameByLinks(LinkType.Download, links);
        return gameDto == null;
    }

    private readonly GameDownloadManager _gameDownloadManager;
    private readonly NavigationManager _navigationManager;
    private readonly GamesUnitOfWork _gamesUoW;
    private readonly TombRaiderLevelInstaller _levelInstaller;
    private readonly TombRaiderEngineDetector _engineDetector;

    [ObservableProperty] private DownloaderSearchPayloadViewModel _searchPayload;
    [ObservableProperty] private ObservableCollection<MultiSourceGameSearchResultMetadataViewModel> _fetchedResults;
    [ObservableProperty] private bool _hasMoreResults;

    protected override void Cancel()
    {
        _gameDownloadManager.CancelCurrentAction();
    }

    public ICommand SearchCmd { get; }

    private async void Search()
    {
        IsBusy = true;
        BusyMessage = "Avvio ricerca...";
        FetchedResults = new ObservableCollection<MultiSourceGameSearchResultMetadataViewModel>();
        try
        {
            var games = await _gameDownloadManager.GetGames(SearchPayload.ToDto());
            var downloadLinks = games.SelectMany(g => g.Sources).Select(s => s.DownloadLink) /*TODO Remove this*/
                .Where(s => !string.IsNullOrWhiteSpace(s)).ToList();
            var installedGames = _gamesUoW.GetGamesByLinksDictionary(LinkType.Download, downloadLinks);
            foreach (var game in games)
            {
                if (game.DownloadLink == null) continue;
                if (installedGames.TryGetValue(game.DownloadLink, out var installedGame))
                {
                    game.InstalledGame = installedGame.ToViewModel();
                }
            }

            Dispatcher.UIThread.Invoke(() => FetchedResults = games.ToObservableCollection());
            HasMoreResults = _gameDownloadManager.HasMoreResults();
//            FetchedResults = games.ToObservableCollection();
        }
        catch (OperationCanceledException)
        {
            FetchedResults = new ObservableCollection<MultiSourceGameSearchResultMetadataViewModel>();
        }

        IsBusy = false;
    }

    public ICommand LoadMoreCmd { get; }

    private async void LoadMore()
    {
        IsBusy = true;
        BusyMessage = "Caricamento in corso...";
        var nextPage = await _gameDownloadManager.FetchNextPage();
        _gameDownloadManager.Merge(FetchedResults, nextPage);
        var gamesByLinks =
            _gamesUoW.GetGamesByLinksDictionary(LinkType.Download, nextPage.Select(p => p.DownloadLink).ToList());
        foreach (var game in FetchedResults.Where(r => r.InstalledGame == null))
        {
            if (gamesByLinks.TryGetValue(game.DownloadLink, out var dto))
            {
                game.InstalledGame = dto.ToViewModel();
            }
        }

        HasMoreResults = _gameDownloadManager.HasMoreResults();
        IsBusy = false;
    }

    private bool CanLoadMore()
    {
        return _gameDownloadManager.HasMoreResults();
    }

    public ICommand InstallCmd { get; }

    private async Task Install(MultiSourceGameSearchResultMetadataViewModel gameToInstall)
    {
        /*IsBusy = true;
        BusyMessage = "Installing game...".GetLocalizedString();*/
        var downloadPath = await _gameDownloadManager.DownloadGame(gameToInstall, new Progress<DownloadProgressInfo>(
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
        Console.WriteLine(downloadPath);
        var hashCalculator = Ioc.Default.GetRequiredService<GameFileHashCalculator>();
        var hashes = await hashCalculator.CalculateHashes(downloadPath);
        if (_gamesUoW.ExistsHashes(hashes))
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
        var dto = await _gameDownloadManager.FetchDetails(gameToInstall);
        dto.Guid = Guid.NewGuid();
        var installLocation = await _levelInstaller.Install(downloadPath, dto, new Progress<CopyProgressInfo>(a =>
        {
            gameToInstall.TotalBytes = 100;
            gameToInstall.CurrentBytes = a.Percentage.GetValueOrDefault();
        }));
        dto.InstallDate = DateTime.Now;
        dto.InstallDirectory = installLocation;
        var exePath = _engineDetector.GetGameExecutablePath(installLocation);
        dto.ExecutablePath = exePath;
        _gamesUoW.UpsertGame(dto);
        hashes.ForEach(h => h.GameId = dto.Id);
        _gamesUoW.SaveHashes(hashes);
        _gamesUoW.SaveLink(new GameLinkDto()
            { Link = gameToInstall.DownloadLink, LinkType = LinkType.Download, GameId = dto.Id });
        _gamesUoW.SaveLink(new GameLinkDto()
            { Link = gameToInstall.DetailsLink, LinkType = LinkType.Details, GameId = dto.Id });
        if (gameToInstall.HasReviews)
        {
            _gamesUoW.SaveLink(new GameLinkDto()
                { Link = gameToInstall.ReviewsLink, LinkType = LinkType.Reviews, GameId = dto.Id });
        }

        if (gameToInstall.HasWalkthrough)
        {
            _gamesUoW.SaveLink(new GameLinkDto()
                { Link = gameToInstall.WalkthroughLink, LinkType = LinkType.Walkthrough, GameId = dto.Id });
        }

        _gamesUoW.Save();

        //TODO _gameDownloadManager.Downloaders
    }

    public ICommand OpenCmd { get; }

    private async void Open(IGameSearchResultMetadata gameToOpen)
    {
        IsBusy = true;
        var details = await _gameDownloadManager.FetchDetails(gameToOpen);
        if (details != null)
        {
            var detailsViewModel = details.ToViewModel();
            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            if (gameToOpen.TitlePic == null)
            {
                gameToOpen.TitlePic = detailsViewModel.TitlePic;
            }

            var uow = Ioc.Default.GetRequiredService<GamesUnitOfWork>();
            var vm = new GameDetailsViewModel(uow,
                new GameWithStatsViewModel(uow, LocalizationManager) { GameMetadata = detailsViewModel },
                LocalizationManager);
            IsBusy = false;
            _navigationManager.NavigateTo(vm);
            return;
        }

        IsBusy = false;
    }
}