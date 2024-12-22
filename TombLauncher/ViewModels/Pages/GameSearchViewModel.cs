using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TombLauncher.Services;

namespace TombLauncher.ViewModels.Pages;

public partial class GameSearchViewModel : PageViewModel
{
    public GameSearchViewModel(GameSearchService searchService)
    {
        _gameSearchService = searchService;
        SearchPayload = new DownloaderSearchPayloadViewModel();
        SearchCmd = new AsyncRelayCommand(Search);
        OpenCmd = new AsyncRelayCommand<MultiSourceGameSearchResultMetadataViewModel>(Open);
        LoadMoreCmd = new AsyncRelayCommand(LoadMore);
        IsCancelable = true;
        
    }

    private GameSearchService _gameSearchService;

    [ObservableProperty] private DownloaderSearchPayloadViewModel _searchPayload;
    [ObservableProperty] private ObservableCollection<MultiSourceGameSearchResultMetadataViewModel> _fetchedResults;
    [ObservableProperty] private bool _hasMoreResults;
    

    protected override void Cancel()
    {
        _gameSearchService.Cancel();
    }

    public ICommand SearchCmd { get; }

    private async Task Search()
    {
        await _gameSearchService.Search(this);
    }

    public ICommand LoadMoreCmd { get; }

    private async Task LoadMore()
    {
        await _gameSearchService.LoadMore(this);
    }

    public ICommand OpenCmd { get; }

    private async Task Open(MultiSourceGameSearchResultMetadataViewModel gameToOpen)
    {
        await _gameSearchService.Open(this, gameToOpen);
    }
}