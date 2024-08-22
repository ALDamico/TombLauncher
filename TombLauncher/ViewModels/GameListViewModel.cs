using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using JamSoft.AvaloniaUI.Dialogs;
using TombLauncher.Database.UnitOfWork;
using TombLauncher.Extensions;
using TombLauncher.Navigation;

namespace TombLauncher.ViewModels;

public partial class GameListViewModel : PageViewModel
{
    private readonly GamesUnitOfWork _gamesUnitOfWork;
    [ObservableProperty] private ObservableCollection<GameDataGridRowViewModel> _games;

    public GameListViewModel(GamesUnitOfWork gamesUoW, NavigationManager navigationManager)
    {
        _gamesUnitOfWork = gamesUoW;
        _navigationManager = navigationManager;
        AddGameCmd = new RelayCommand(AddGame);
        Initialize += OnInit;
        navigationManager.OnNavigated += OnInit;
    }
    
    private readonly NavigationManager _navigationManager;

    private async void OnInit()
    {
        IsBusy = true;
        Games = _gamesUnitOfWork.GetGames()
            .ToViewModels().Select(vm => new GameDataGridRowViewModel(_navigationManager){GameMetadata = vm}).ToObservableCollection();
        IsBusy = false;
    }
    
    public ICommand AddGameCmd { get; }

    private void AddGame()
    {
        _navigationManager.NavigateTo(new NewGameViewModel(_gamesUnitOfWork, Ioc.Default.GetService<IDialogService>()));
    }
}