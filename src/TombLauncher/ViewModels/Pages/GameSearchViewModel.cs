using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TombLauncher.Contracts.Downloaders;
using TombLauncher.Services;

namespace TombLauncher.ViewModels.Pages;

public partial class GameSearchViewModel : PageViewModel
{
    private readonly GameSearchService _gameSearchService;

    [ObservableProperty] private DownloaderSearchPayloadViewModel _searchPayload;
    [ObservableProperty] private ObservableCollection<MultiSourceGameSearchResultMetadataViewModel> _fetchedResults = new();
    [ObservableProperty] private bool _hasMoreResults;
    [ObservableProperty] private Vector _scrollViewerOffset;
    [ObservableProperty] private bool _hasSearched;
    [ObservableProperty] private int _resultCount;
    [ObservableProperty] private bool _showEmptyState;
    [ObservableProperty] private bool _showResults;
    [ObservableProperty] private string _loadMoreFeedback = string.Empty;

    // Pagination state — owned by the ViewModel, populated by GameSearchService
    [ObservableProperty] private int _currentPage;
    [ObservableProperty] private int _maxTotalPages;
    internal DownloaderSearchPayload? LastSearchPayload;
    internal IReadOnlyList<IGameDownloader>? LastSearchDownloaders;

    public GameSearchViewModel(GameSearchService gameSearchService)
    {
        _gameSearchService = gameSearchService;
        SearchPayload = new DownloaderSearchPayloadViewModel();
        SearchPayload.PropertyChanged += OnSearchPayloadPropertyChanged;
        IsCancelable = true;
    }

    protected override void Cancel()
    {
        _gameSearchService.Cancel();
    }

    [RelayCommand(AllowConcurrentExecutions = false)]
    private async Task Search()
    {
        await _gameSearchService.Search(this);
        HasSearched = true;
        ResultCount = FetchedResults.Count;
        ShowEmptyState = HasSearched && ResultCount == 0;
        ShowResults = ResultCount > 0;
        LoadMoreFeedback = string.Empty;
    }

    [RelayCommand(AllowConcurrentExecutions = false)]
    private async Task HandleKeyUp(KeyEventArgs? keyEventArgs)
    {
        if (keyEventArgs?.Key == Key.Enter)
            await Search();
    }

    [RelayCommand(AllowConcurrentExecutions = false)]
    private async Task LoadMore()
    {
        try
        {
            await _gameSearchService.LoadMore(this);
        }
        catch (OperationCanceledException)
        {
        }
        finally
        {
            ResultCount = FetchedResults.Count;
            ShowEmptyState = HasSearched && ResultCount == 0;
            ShowResults = ResultCount > 0;
        }
    }
    
    [RelayCommand(AllowConcurrentExecutions = false)]
    private async Task Open(MultiSourceGameSearchResultMetadataViewModel? gameToOpen)
    {
        if (gameToOpen != null)
            await _gameSearchService.Open(this, gameToOpen);
    }

    [RelayCommand(CanExecute = nameof(CanClearFilters))]
    private void ClearFilters()
    {
        SearchPayload.ClearFilters();
    }

    private bool CanClearFilters()
    {
        return SearchPayload.HasActiveFilters;
    }

    private void OnSearchPayloadPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(DownloaderSearchPayloadViewModel.HasActiveFilters))
        {
            ClearFiltersCommand.NotifyCanExecuteChanged();
        }
    }
}