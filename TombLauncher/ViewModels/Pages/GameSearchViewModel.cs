using System.Collections.ObjectModel;
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
    [ObservableProperty] private ObservableCollection<MultiSourceGameSearchResultMetadataViewModel> _fetchedResults;
    [ObservableProperty] private bool _hasMoreResults;
    [ObservableProperty] private Vector _scrollViewerOffset;

    protected override Task RaiseInitialize()
    {
        SetBusy(true);
        _gameSearchService = Ioc.Default.GetRequiredService<GameSearchService>();
        SearchPayload = new DownloaderSearchPayloadViewModel();
        SearchCmd = new AsyncRelayCommand(Search);
        HandleKeyUpCmd = new AsyncRelayCommand<KeyEventArgs>(HandleKeyUp);
        OpenCmd = new AsyncRelayCommand<MultiSourceGameSearchResultMetadataViewModel>(Open);
        LoadMoreCmd = new AsyncRelayCommand(LoadMore);
        IsCancelable = true;
        SetBusy(false);
        return base.RaiseInitialize();
    }


    protected override void Cancel()
    {
        _gameSearchService.Cancel();
    }

    [ObservableProperty] private ICommand _searchCmd;

    private async Task Search()
    {
        await _gameSearchService.Search(this);
    }

    [ObservableProperty] private ICommand _handleKeyUpCmd;

    private async Task HandleKeyUp(KeyEventArgs keyEventArgs)
    {
        if (keyEventArgs.Key == Key.Enter)
            await Search();
    }

    [ObservableProperty] private ICommand _loadMoreCmd;

    private async Task LoadMore()
    {
        await _gameSearchService.LoadMore(this);
    }

    [ObservableProperty] private ICommand _openCmd;

    private async Task Open(MultiSourceGameSearchResultMetadataViewModel gameToOpen)
    {
        await _gameSearchService.Open(this, gameToOpen);
    }
}