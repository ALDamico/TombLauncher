using System.Collections.ObjectModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Material.Icons;
using TombLauncher.Localization.Extensions;
using TombLauncher.Navigation;
using TombLauncher.Services;

namespace TombLauncher.ViewModels.Pages;

public partial class GameListViewModel : PageViewModel
{
    [ObservableProperty] private ObservableCollection<GameWithStatsViewModel> _games;
    [ObservableProperty] private GameWithStatsViewModel _selectedGame;

    public GameListViewModel(GameListService gameListService) 
    {
        _gameListService = gameListService;
        AddGameCmd = new RelayCommand(AddGame);
        UninstallCmd = new RelayCommand<GameWithStatsViewModel>(Uninstall);
        OpenCmd = new RelayCommand<GameWithStatsViewModel>(Open);
        OpenSearchCmd = new RelayCommand(OpenSearch);
        _gameListService.NavigationManager.OnNavigated += OnInit;
        TopBarCommands.Add(new CommandViewModel(){Command = OpenSearchCmd, Icon = MaterialIconKind.Search, Tooltip = "Open search".GetLocalizedString()});
        TopBarCommands.Add(new CommandViewModel(){Command = AddGameCmd, Icon = MaterialIconKind.Plus, Tooltip = "Add".GetLocalizedString()});
    }

    private GameListService _gameListService;

    private async void OnInit()
    {
        Games = await _gameListService.FetchGames(this);
        ClearBusy();
    }
    
    public ICommand AddGameCmd { get; }

    private void AddGame()
    {
        _gameListService.AddGame();
    }
    
    public ICommand OpenSearchCmd { get; }

    private void OpenSearch()
    {
        _gameListService.OpenSearch();
    }
    
    public ICommand OpenCmd { get; }

    private void Open(GameWithStatsViewModel game)
    {
        game.OpenCmd.Execute(null);
    }
    
    public ICommand UninstallCmd { get; }

    private async void Uninstall(GameWithStatsViewModel game)
    {
        await _gameListService.Uninstall(this, game);
    }
}