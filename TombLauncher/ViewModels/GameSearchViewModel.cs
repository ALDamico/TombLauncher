using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Avalonia.Threading;
using AvaloniaEdit.Utils;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using TombLauncher.Database.UnitOfWork;
using TombLauncher.Extensions;
using TombLauncher.Installers.Downloaders;
using TombLauncher.Localization;
using TombLauncher.Navigation;

namespace TombLauncher.ViewModels;

public partial class GameSearchViewModel : PageViewModel
{
    public GameSearchViewModel(LocalizationManager localizationManager, GameDownloadManager gameDownloadManager, NavigationManager navigationManager) : base(localizationManager)
    {
        _searchPayload = new DownloaderSearchPayloadViewModel();
        _navigationManager = navigationManager;
        _gameDownloadManager = gameDownloadManager;
        SearchCmd = new RelayCommand(Search);
        OpenCmd = new RelayCommand<GameSearchResultMetadataViewModel>(Open);
        LoadMoreCmd = new RelayCommand(LoadMore, CanLoadMore);
        IsCancelable = true;
    }

    private readonly GameDownloadManager _gameDownloadManager;
    private readonly NavigationManager _navigationManager;

    [ObservableProperty] private DownloaderSearchPayloadViewModel _searchPayload;
    [ObservableProperty] private ObservableCollection<GameSearchResultMetadataViewModel> _fetchedResults;
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
        FetchedResults = new ObservableCollection<GameSearchResultMetadataViewModel>();
        try
        {
            var games = await _gameDownloadManager.GetGames(SearchPayload.ToDto());
            Dispatcher.UIThread.Invoke(() => FetchedResults = games.ToObservableCollection());
            HasMoreResults = _gameDownloadManager.HasMoreResults();
//            FetchedResults = games.ToObservableCollection();
        }
        catch (OperationCanceledException)
        {
            FetchedResults = new ObservableCollection<GameSearchResultMetadataViewModel>();
        }
        IsBusy = false;
    }
    
    public ICommand LoadMoreCmd { get; }

    private async void LoadMore()
    {
        IsBusy = true;
        BusyMessage = "Caricamento in corso...";
        FetchedResults.AddRange(await _gameDownloadManager.FetchNextPage());
        HasMoreResults = _gameDownloadManager.HasMoreResults();
        IsBusy = false;
    }

    private bool CanLoadMore()
    {
        return _gameDownloadManager.HasMoreResults();
    }
    
    public ICommand OpenCmd { get; }

    private async void Open(GameSearchResultMetadataViewModel gameToOpen)
    {
        IsBusy = true;
        var details = await _gameDownloadManager.FetchDetails(gameToOpen);
        if (details != null)
        {
            var uow = Ioc.Default.GetRequiredService<GamesUnitOfWork>();
            var vm = new GameDetailsViewModel(uow,
                new GameWithStatsViewModel(uow, LocalizationManager) { GameMetadata = details.ToViewModel() },
                LocalizationManager);
            IsBusy = false;
            _navigationManager.NavigateTo(vm);
            return;
        }

        IsBusy = false;
    }
}