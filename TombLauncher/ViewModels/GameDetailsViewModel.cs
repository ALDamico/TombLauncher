using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TombLauncher.Localization;
using TombLauncher.Services;

namespace TombLauncher.ViewModels;

public partial class GameDetailsViewModel : PageViewModel
{
    public GameDetailsViewModel(GameDetailsService gameDetailsService, GameWithStatsViewModel game, LocalizationManager localizationManager) : base(localizationManager)
    {
        _gameDetailsService = gameDetailsService;
        _game = game;
        BrowseFolderCmd = new RelayCommand(BrowseFolder);
        UninstallCmd = new RelayCommand(Uninstall, CanUninstall);
    }

    private readonly GameDetailsService _gameDetailsService;
    [ObservableProperty] private GameWithStatsViewModel _game;
    public ICommand BrowseFolderCmd { get; }

    private void BrowseFolder()
    {
        _gameDetailsService.OpenGameFolder(Game.GameMetadata.InstallDirectory);
    }
    
    public ICommand UninstallCmd { get; }

    private void Uninstall()
    {
        _gameDetailsService.Uninstall(_game.GameMetadata.InstallDirectory, _game.GameMetadata.Id);
    }

    private bool CanUninstall()
    {
        return _gameDetailsService.CanUninstall(_game.GameMetadata.InstallDirectory);
    }
}