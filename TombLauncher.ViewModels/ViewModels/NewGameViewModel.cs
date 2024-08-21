using CommunityToolkit.Mvvm.ComponentModel;
using TombLauncher.Database.Entities;
using TombLauncher.Database.UnitOfWork;

namespace TombLauncher.ViewModels.ViewModels;

public partial class NewGameViewModel : PageViewModel
{
    public NewGameViewModel(GamesUnitOfWork gamesUnitOfWork)
    {
        _gamesUoW = gamesUnitOfWork;
    }

    private GamesUnitOfWork _gamesUoW;
    [ObservableProperty] private GameMetadataViewModel _gameMetadata;

    protected override void SaveInner()
    {
        IsBusy = true;

        IsBusy = false;
    }
}