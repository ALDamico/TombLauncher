using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using TombLauncher.Database.UnitOfWork;
using TombLauncher.ExtensionMethods;
using TombLauncher.ViewModels.Extensions;

namespace TombLauncher.ViewModels.ViewModels;

public partial class GameListViewModel : ViewModelBase
{
    public event Action Init;
    private readonly GamesUnitOfWork _gamesUnitOfWork;
    [ObservableProperty] private ObservableCollection<GameMetadataViewModel> _games;

    public GameListViewModel(GamesUnitOfWork gamesUoW)
    {
        _gamesUnitOfWork = gamesUoW;
        Init += OnInit;
    }

    private async void OnInit()
    {
        Games = _gamesUnitOfWork.Games.GetAll().AsEnumerable().ToViewModels().ToObservableCollection();
    }
}