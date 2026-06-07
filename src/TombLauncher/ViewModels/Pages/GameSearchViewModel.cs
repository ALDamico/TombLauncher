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

    [ObservableProperty]
    public partial DownloaderSearchPayloadViewModel SearchPayload { get; set; }

    [ObservableProperty]
    public partial ObservableCollection<MultiSourceGameSearchResultMetadataViewModel> FetchedResults { get; set; } = [];

    [ObservableProperty]
    public partial bool HasMoreResults { get; set; }

    [ObservableProperty]
    public partial Vector ScrollViewerOffset { get; set; }

    [ObservableProperty]
    public partial bool HasSearched { get; set; }

    [ObservableProperty]
    public partial int ResultCount { get; set; }

    [ObservableProperty]
    public partial bool ShowEmptyState { get; set; }

    [ObservableProperty]
    public partial bool ShowResults { get; set; }

    [ObservableProperty]
    public partial string LoadMoreFeedback { get; set; } = string.Empty;

    // Pagination state — owned by the ViewModel, populated by GameSearchService
    [ObservableProperty]
    public partial int CurrentPage { get; set; }

    [ObservableProperty]
    public partial int MaxTotalPages { get; set; }

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