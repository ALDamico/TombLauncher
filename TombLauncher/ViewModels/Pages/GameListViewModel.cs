using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using Material.Icons;
using TombLauncher.Localization.Extensions;
using TombLauncher.Services;

namespace TombLauncher.ViewModels.Pages;

public partial class GameListViewModel : PageViewModel
{
    [ObservableProperty] private ObservableCollection<GameWithStatsViewModel> _games = new ObservableCollection<GameWithStatsViewModel>();
    [ObservableProperty] private GameWithStatsViewModel? _selectedGame;
    [ObservableProperty] private bool _showAsGrid;

    private GameListService _gameListService;

    public GameListViewModel(GameListService gameListService)
    {
        _gameListService = gameListService;
    }

    public override async Task OnNavigatedTo(object parameter)
    {
        SetBusy("Fetching games...");
        _gameListService.ApplySettings(this);
        AddGameCmd = new AsyncRelayCommand(AddGame);
        UninstallCmd = new RelayCommand<GameWithStatsViewModel>(Uninstall);
        OpenCmd = new RelayCommand<GameWithStatsViewModel>(Open);
        OpenSearchCmd = new AsyncRelayCommand(OpenSearch);
        PlayCmd = new RelayCommand<GameWithStatsViewModel>(Play);
        AddToFavouritesCmd = new AsyncRelayCommand<GameWithStatsViewModel>(AddToFavourites);
        MarkUnmarkCompletedCmd = new AsyncRelayCommand<GameWithStatsViewModel>(MarkUnmarkCompleted);
        InitTopBarCommands();
        Games = await _gameListService.FetchGames(this);
        ClearBusy();
    }

    private void InitTopBarCommands()
    {
        TopBarCommands.Clear();
        TopBarCommands.Add(new CommandViewModel()
        {
            Command = OpenSearchCmd!,
            Icon = MaterialIconKind.Search,
            Tooltip = "Open search".GetLocalizedString()
        });
        TopBarCommands.Add(new CommandViewModel()
        {
            Command = AddGameCmd!,
            Icon = MaterialIconKind.Plus,
            Tooltip = "Add".GetLocalizedString()
        });
    }

    public IAsyncRelayCommand? AddGameCmd { get; private set; }

    private async Task AddGame()
    {
        await _gameListService.AddGame();
    }

    public ICommand? OpenSearchCmd { get; private set; }

    private async Task OpenSearch()
    {
        await _gameListService.OpenSearch();
    }

    public ICommand? PlayCmd { get; private set; }

    private void Play(GameWithStatsViewModel? game)
    {
        game?.PlayCmd?.Execute(null);
    }

    public ICommand? OpenCmd { get; private set; }

    private void Open(GameWithStatsViewModel? game)
    {
        game?.OpenCmd?.Execute(null);
    }

    public ICommand? UninstallCmd { get; private set; }

    private async void Uninstall(GameWithStatsViewModel? game)
    {
        if (game != null)
            await _gameListService.Uninstall(this, game);
    }

    public ICommand? AddToFavouritesCmd { get; private set; }

    private async Task AddToFavourites(GameWithStatsViewModel? game)
    {
        if (game?.MarkGameAsFavouriteCmd != null)
            await game.MarkGameAsFavouriteCmd.ExecuteAsync(null);
    }

    public ICommand? MarkUnmarkCompletedCmd { get; private set; }

    private async Task MarkUnmarkCompleted(GameWithStatsViewModel? game)
    {
        if (game?.MarkGameAsCompletedCmd != null)
            await game.MarkGameAsCompletedCmd.ExecuteAsync(null);
    }
}