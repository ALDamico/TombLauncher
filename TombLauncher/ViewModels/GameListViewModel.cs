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
    [ObservableProperty] private ObservableCollection<GameWithStatsViewModel> _games;

    public GameListViewModel(GamesUnitOfWork gamesUoW, NavigationManager navigationManager)
    {
        _gamesUnitOfWork = gamesUoW;
        AddGameCmd = new RelayCommand(AddGame);
        navigationManager.OnNavigated += OnInit;
    }

    private async void OnInit()
    {
        SetBusy(true, "Loading games...");
        Games =
            _gamesUnitOfWork.GetGamesWithStats().Select(dto =>
                new GameWithStatsViewModel(_gamesUnitOfWork)
                {
                    GameMetadata = dto.GameMetadata.ToViewModel(), 
                    LastPlayed = dto.LastPlayed,
                    TotalPlayedTime = dto.TotalPlayTime
                }).ToObservableCollection();
        ClearBusy();
    }
    
    public ICommand AddGameCmd { get; }

    private void AddGame()
    {
        Program.NavigationManager.NavigateTo(new NewGameViewModel(_gamesUnitOfWork, Ioc.Default.GetService<IDialogService>()));
    }
}