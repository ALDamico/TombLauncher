using System.Diagnostics;
using CommunityToolkit.Mvvm.DependencyInjection;
using JamSoft.AvaloniaUI.Dialogs;
using TombLauncher.Contracts.Localization;
using TombLauncher.Data.Database.UnitOfWork;
using TombLauncher.Extensions;
using TombLauncher.Localization;
using TombLauncher.Navigation;
using TombLauncher.ViewModels;
using TombLauncher.ViewModels.Pages;

namespace TombLauncher.Services;

public class GameWithStatsService : IViewService
{
    public GameWithStatsService(GamesUnitOfWork gamesUnitOfWork, 
        ILocalizationManager localizationManager, 
        NavigationManager navigationManager, 
        IMessageBoxService messageBoxService, 
        IDialogService dialogService)
    {
        GamesUnitOfWork = gamesUnitOfWork;
        LocalizationManager = localizationManager;
        NavigationManager = navigationManager;
        MessageBoxService = messageBoxService;
        DialogService = dialogService;
    }
    public GamesUnitOfWork GamesUnitOfWork { get; }
    public ILocalizationManager LocalizationManager { get; }
    public NavigationManager NavigationManager { get; }
    public IMessageBoxService MessageBoxService { get; }
    public IDialogService DialogService { get; }

    public void OpenGame(GameWithStatsViewModel game)
    {
        var gameDetailsService = Ioc.Default.GetRequiredService<GameDetailsService>();
        NavigationManager.NavigateTo(new GameDetailsViewModel(gameDetailsService, game));
    }

    public void PlayGame(GameWithStatsViewModel game)
    {
        var currentPage = NavigationManager.GetCurrentPage();
        currentPage.IsBusy = true;
        currentPage.BusyMessage = LocalizationManager.GetLocalizedString("Starting GAMENAME", game.GameMetadata.Title);
        var process = new Process()
        {
            StartInfo = new ProcessStartInfo(game.GameMetadata.ExecutablePath)
            {
                WorkingDirectory = game.GameMetadata.InstallDirectory,
                UseShellExecute = true,
            },
            EnableRaisingEvents = true
        };
        process.Exited += (sender, args) => OnGameExited(game, sender as Process);
        process.Start();
    }
    
    private void OnGameExited(GameWithStatsViewModel game, Process process)
    {
        var currentPage = NavigationManager.GetCurrentPage();
        currentPage.BusyMessage = LocalizationManager["Saving play session..."];
        GamesUnitOfWork.AddPlaySessionToGame(game.GameMetadata.ToDto(), process.StartTime, process.ExitTime);
        GamesUnitOfWork.Save();
        NavigationManager.RequestRefresh();
        currentPage.IsBusy = false;
        currentPage.BusyMessage = null;
    }
}