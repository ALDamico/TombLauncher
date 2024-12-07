using System.Diagnostics;
using System.IO;
using JamSoft.AvaloniaUI.Dialogs;
using TombLauncher.Data.Database.UnitOfWork;
using TombLauncher.Localization;
using TombLauncher.Navigation;

namespace TombLauncher.Services;

public class GameDetailsService : IViewService
{
    public GameDetailsService(GamesUnitOfWork gamesUnitOfWork, LocalizationManager localizationManager, NavigationManager navigationManager, IMessageBoxService messageBoxService, IDialogService dialogService)
    {
        GamesUnitOfWork = gamesUnitOfWork;
        LocalizationManager = localizationManager;
        NavigationManager = navigationManager;
        MessageBoxService = messageBoxService;
        DialogService = dialogService;
    }
    public GamesUnitOfWork GamesUnitOfWork { get; set; }
    public LocalizationManager LocalizationManager { get; set; }
    public NavigationManager NavigationManager { get; set; }
    public IMessageBoxService MessageBoxService { get; }
    public IDialogService DialogService { get; }

    public void OpenGameFolder(string gameFolder)
    {
        Process.Start("explorer", gameFolder);
    }

    public bool CanUninstall(string gameFolder)
    {
        return !string.IsNullOrWhiteSpace(gameFolder);
    }

    public void Uninstall(string installDir, int gameId)
    {
        Directory.Delete(installDir, true);
        GamesUnitOfWork.DeleteGameById(gameId);
        GamesUnitOfWork.Save();
        NavigationManager.GoBack();
    }
}