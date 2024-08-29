using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TombLauncher.Extensions;
using TombLauncher.Installers.Downloaders;
using TombLauncher.Localization;

namespace TombLauncher.ViewModels;

public partial class GameSearchViewModel : PageViewModel
{
    public GameSearchViewModel(LocalizationManager localizationManager, GameDownloadManager gameDownloadManager) : base(localizationManager)
    {
        _searchPayload = new DownloaderSearchPayloadViewModel();
        _gameDownloadManager = gameDownloadManager;
        SearchCmd = new RelayCommand(Search);
        IsCancelable = true;
    }

    private readonly GameDownloadManager _gameDownloadManager;

    [ObservableProperty] private DownloaderSearchPayloadViewModel _searchPayload;
    [ObservableProperty] private ObservableCollection<GameSearchResultMetadataViewModel> _fetchedResults;
    protected override void Cancel()
    {
        _gameDownloadManager.CancelCurrentAction();
    }

    public ICommand SearchCmd { get; }

    private async void Search()
    {
        IsBusy = true;
        BusyMessage = "Avvio ricerca...";
        try
        {
            var games = await _gameDownloadManager.GetGames(SearchPayload.ToDto());
            FetchedResults = games.ToObservableCollection();
        }
        catch (OperationCanceledException)
        {
            FetchedResults = new ObservableCollection<GameSearchResultMetadataViewModel>();
        }
        IsBusy = false;
    }
}