using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TombLauncher.Database.UnitOfWork;
using TombLauncher.ExtensionMethods;
using TombLauncher.ViewModels.Extensions;

namespace TombLauncher.ViewModels.ViewModels;

public partial class GameListViewModel : PageViewModel
{
    private readonly GamesUnitOfWork _gamesUnitOfWork;
    [ObservableProperty] private ObservableCollection<GameMetadataViewModel> _games;

    public GameListViewModel(GamesUnitOfWork gamesUoW)
    {
        _gamesUnitOfWork = gamesUoW;
        AddGameCmd = new RelayCommand(AddGame);
        Initialize += OnInit;
    }

    private async void OnInit()
    {
        IsBusy = true;
        Games = _gamesUnitOfWork.GetGames().ToViewModels().ToObservableCollection();
        IsBusy = false;
    }
    
    public ICommand AddGameCmd { get; }

    private void AddGame()
    {
        
    }
}