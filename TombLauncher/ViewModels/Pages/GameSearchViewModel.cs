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
        _searchPayload = new DownloaderSearchPayloadViewModel();
        SearchCmd = new AsyncRelayCommand(Search);
        OpenCmd = new AsyncRelayCommand<IGameSearchResultMetadata>(Open);
        LoadMoreCmd = new AsyncRelayCommand(LoadMore);
        IsCancelable = true;
        InstallCmd = new AsyncRelayCommand<MultiSourceGameSearchResultMetadataViewModel>(Install, CanInstall);
    }

    private GameSearchService _gameSearchService;

    private bool CanInstall(MultiSourceGameSearchResultMetadataViewModel obj)
    {
        return _gameSearchService.CanInstall(obj);
    }

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

    public ICommand InstallCmd { get; }

    private async Task Install(MultiSourceGameSearchResultMetadataViewModel gameToInstall)
    {
        await _gameSearchService.Install(gameToInstall);
    }

    public ICommand OpenCmd { get; }

    private async Task Open(IGameSearchResultMetadata gameToOpen)
    {
        await _gameSearchService.Open(this, gameToOpen);
    }
}