using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using TombLauncher.Services;

namespace TombLauncher.ViewModels.Pages;

public partial class GameSearchViewModel : PageViewModel
{
    public GameSearchViewModel()
    {
        _gameSearchService = Ioc.Default.GetRequiredService<GameSearchService>();
        SearchPayload = new DownloaderSearchPayloadViewModel();
        SearchCmd = new AsyncRelayCommand(Search);
        OpenCmd = new AsyncRelayCommand<MultiSourceGameSearchResultMetadataViewModel>(Open);
        LoadMoreCmd = new AsyncRelayCommand(LoadMore);
        IsCancelable = true;
    }

    private readonly GameSearchService _gameSearchService;

    [ObservableProperty] private DownloaderSearchPayloadViewModel _searchPayload;
    [ObservableProperty] private ObservableCollection<MultiSourceGameSearchResultMetadataViewModel> _fetchedResults;
    [ObservableProperty] private bool _hasMoreResults;
    [ObservableProperty] private Vector _scrollViewerOffset;
    

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