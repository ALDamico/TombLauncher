using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using IconPacks.Avalonia.RemixIcon;
using TombLauncher.Localization.Extensions;
using TombLauncher.Services;

namespace TombLauncher.ViewModels.Pages;

public partial class GameListViewModel : PageViewModel
{
    [ObservableProperty]
    public partial ObservableCollection<GameWithStatsViewModel>? Games { get; set; }

    [ObservableProperty]
    public partial GameWithStatsViewModel? SelectedGame { get; set; }

    [ObservableProperty]
    public partial bool ShowAsGrid { get; set; }

    private readonly GameListService _gameListService;

    public GameListViewModel(GameListService gameListService)
    {
        _gameListService = gameListService;
    }

    public override async Task OnNavigatedTo(object parameter)
    {
        using (BusyScope("Fetching games..."))
        {
            _gameListService.ApplySettings(this);
            InitTopBarCommands();
            Games = await _gameListService.FetchGames(this);
        }
    }

    private void InitTopBarCommands()
    {
        TopBarCommands.Clear();
        TopBarCommands.Add(new CommandViewModel()
        {
            Command = OpenSearchCommand,
            Icon = PackIconRemixIconKind.Search2Line,
            Tooltip = "OPEN_SEARCH".GetLocalizedString()
        });
        TopBarCommands.Add(new CommandViewModel()
        {
            Command = AddGameCommand,
            Icon = PackIconRemixIconKind.AddLargeLine,
            Tooltip = "ADD".GetLocalizedString()
        });
    }
    
    [RelayCommand(AllowConcurrentExecutions = false)]
    private async Task AddGame()
    {
        await _gameListService.AddGame();
    }

    [RelayCommand(AllowConcurrentExecutions = false)]
    private async Task OpenSearch()
    {
        await _gameListService.OpenSearch();
    }

    [RelayCommand]
    private void Play(GameWithStatsViewModel? game)
    {
        game?.PlayCommand.Execute(null);
    }

    [RelayCommand]
    private void Open(GameWithStatsViewModel? game)
    {
        game?.OpenCommand.Execute(null);
    }

    [RelayCommand(AllowConcurrentExecutions = false)]
    private async Task Uninstall(GameWithStatsViewModel? game)
    {
        if (game == null)
            return;
        await _gameListService.Uninstall(this, game);
    }

    [RelayCommand]
    private async Task AddToFavourites(GameWithStatsViewModel? game)
    {
        if (game?.ToggleFavouriteCommand != null)
            await game.ToggleFavouriteCommand.ExecuteAsync(null);
    }

    [RelayCommand]
    private async Task ToggleCompleted(GameWithStatsViewModel? game)
    {
        if (game?.ToggleCompletedCommand != null)
            await game.ToggleCompletedCommand.ExecuteAsync(null);
    }
}