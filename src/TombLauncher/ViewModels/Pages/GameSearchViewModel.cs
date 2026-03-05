using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia;
using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using TombLauncher.Services;

namespace TombLauncher.ViewModels.Pages;

public partial class GameSearchViewModel : PageViewModel
{
    private GameSearchService _gameSearchService;

    [ObservableProperty] private DownloaderSearchPayloadViewModel _searchPayload;
    [ObservableProperty] private ObservableCollection<MultiSourceGameSearchResultMetadataViewModel> _fetchedResults = new ObservableCollection<MultiSourceGameSearchResultMetadataViewModel>();
    [ObservableProperty] private bool _hasMoreResults;
    [ObservableProperty] private Vector _scrollViewerOffset;
    [ObservableProperty] private bool _hasSearched;
    [ObservableProperty] private int _resultCount;
    [ObservableProperty] private bool _showEmptyState;
    [ObservableProperty] private bool _showResults;
    [ObservableProperty] private string _loadMoreFeedback = string.Empty;

    public GameSearchViewModel(GameSearchService gameSearchService)
    {
        _gameSearchService = gameSearchService;
        SearchPayload = new DownloaderSearchPayloadViewModel();
        SearchPayload.PropertyChanged += OnSearchPayloadPropertyChanged;
        SearchCmd = new AsyncRelayCommand(Search);
        HandleKeyUpCmd = new AsyncRelayCommand<KeyEventArgs?>(HandleKeyUp);
        OpenCmd = new AsyncRelayCommand<MultiSourceGameSearchResultMetadataViewModel?>(Open);
        LoadMoreCmd = new AsyncRelayCommand(LoadMore);
        ClearFiltersCmd = new RelayCommand(ClearFilters, CanClearFilters);
        IsCancelable = true;
    }

    public override Task OnNavigatedTo(object parameter)
    {
        return Task.CompletedTask;
    }


    protected override void Cancel()
    {
        _gameSearchService.Cancel();
    }

    [ObservableProperty] private ICommand _searchCmd;

    private async Task Search()
    {
        await _gameSearchService.Search(this);
        HasSearched = true;
        ResultCount = FetchedResults?.Count ?? 0;
        ShowEmptyState = HasSearched && ResultCount == 0;
        ShowResults = ResultCount > 0;
        LoadMoreFeedback = string.Empty;
    }

    [ObservableProperty] private ICommand _handleKeyUpCmd;

    private async Task HandleKeyUp(KeyEventArgs? keyEventArgs)
    {
        if (keyEventArgs?.Key == Key.Enter)
            await Search();
    }

    [ObservableProperty] private ICommand _loadMoreCmd;

    private async Task LoadMore()
    {
        await _gameSearchService.LoadMore(this);
        ResultCount = FetchedResults?.Count ?? 0;
        ShowEmptyState = HasSearched && ResultCount == 0;
        ShowResults = ResultCount > 0;
    }

    [ObservableProperty] private ICommand _openCmd;

    private async Task Open(MultiSourceGameSearchResultMetadataViewModel? gameToOpen)
    {
        if (gameToOpen != null)
            await _gameSearchService.Open(this, gameToOpen);
    }

    [ObservableProperty] private ICommand _clearFiltersCmd;

    private void ClearFilters()
    {
        SearchPayload?.ClearFilters();
    }

    private bool CanClearFilters()
    {
        return SearchPayload?.HasActiveFilters == true;
    }

    private void OnSearchPayloadPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(DownloaderSearchPayloadViewModel.HasActiveFilters))
        {
            (ClearFiltersCmd as RelayCommand)?.NotifyCanExecuteChanged();
        }
    }
}