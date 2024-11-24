using System.Diagnostics;
using System.IO;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TombLauncher.Data.Database.UnitOfWork;
using TombLauncher.Extensions;
using TombLauncher.Localization;

namespace TombLauncher.ViewModels;

public partial class GameDetailsViewModel : PageViewModel
{
    public GameDetailsViewModel(GamesUnitOfWork gamesUnitOfWork, GameWithStatsViewModel game, LocalizationManager localizationManager) : base(localizationManager)
    {
        _gamesUnitOfWork = gamesUnitOfWork;
        _game = game;
        BrowseFolderCmd = new RelayCommand(BrowseFolder);
        UninstallCmd = new RelayCommand(Uninstall, CanUninstall);
    }
    [ObservableProperty] private GameWithStatsViewModel _game;
    private readonly GamesUnitOfWork _gamesUnitOfWork;
    public ICommand BrowseFolderCmd { get; }

    private void BrowseFolder()
    {
        Process.Start("explorer", Game.GameMetadata.InstallDirectory);
    }
    
    public ICommand UninstallCmd { get; }

    private void Uninstall()
    {
        var installDir = Game.GameMetadata.InstallDirectory;
        Directory.Delete(installDir, true);
        _gamesUnitOfWork.DeleteGameById(Game.GameMetadata.Id);
        _gamesUnitOfWork.Save();
        Program.NavigationManager.GoBack();
    }

    private bool CanUninstall()
    {
        return !string.IsNullOrWhiteSpace(Game?.GameMetadata.InstallDirectory);
    }
}