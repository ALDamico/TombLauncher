using System.Diagnostics;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TombLauncher.Database.UnitOfWork;
using TombLauncher.Localization;

namespace TombLauncher.ViewModels;

public partial class GameDetailsViewModel : PageViewModel
{
    public GameDetailsViewModel(GamesUnitOfWork gamesUnitOfWork, GameWithStatsViewModel game, LocalizationManager localizationManager) : base(localizationManager)
    {
        _gamesUnitOfWork = gamesUnitOfWork;
        _game = game;
        BrowseFolderCmd = new RelayCommand(BrowseFolder);
    }
    [ObservableProperty] private GameWithStatsViewModel _game;
    private readonly GamesUnitOfWork _gamesUnitOfWork;
    public ICommand BrowseFolderCmd { get; }

    private void BrowseFolder()
    {
        Process.Start("explorer", Game.GameMetadata.InstallDirectory);
    }
}