using System.Collections.Generic;
using System.Diagnostics;
using CommunityToolkit.Mvvm.DependencyInjection;
using JamSoft.AvaloniaUI.Dialogs;
using TombLauncher.Contracts.Localization;
using TombLauncher.Data.Database.UnitOfWork;
using TombLauncher.Extensions;
using TombLauncher.Localization.Extensions;
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
        var settingsService = Ioc.Default.GetRequiredService<SettingsService>();
        NavigationManager.NavigateTo(new GameDetailsViewModel(gameDetailsService, game, settingsService));
    }

    public void PlayGame(GameWithStatsViewModel game)
    {
        var currentPage = NavigationManager.GetCurrentPage();
        currentPage.SetBusy(LocalizationManager.GetLocalizedString("Starting GAMENAME", game.GameMetadata.Title));
        LaunchProcess(game, true);
    }

    public bool CanPlayGame(GameWithStatsViewModel game)
    {
        return !string.IsNullOrWhiteSpace(game.GameMetadata.InstallDirectory);
    }

    private void OnSetupExited()
    {
        var currentPage = NavigationManager.GetCurrentPage();
        currentPage.ClearBusy();
    }
    
    private void OnGameExited(GameWithStatsViewModel game, Process process)
    {
        var currentPage = NavigationManager.GetCurrentPage();
        currentPage.SetBusy(true, "Saving play session...".GetLocalizedString());
        GamesUnitOfWork.AddPlaySessionToGame(game.GameMetadata.ToDto(), process.StartTime, process.ExitTime);
        GamesUnitOfWork.Save();
        NavigationManager.RequestRefresh();
        currentPage.ClearBusy();
    }

    public void LaunchSetup(GameWithStatsViewModel game)
    {
        var currentPage = NavigationManager.GetCurrentPage();
        currentPage.SetBusy(LocalizationManager.GetLocalizedString("Launching setup for GAMENAME", game.GameMetadata.Title));
        LaunchProcess(game, false, new List<string>(){"-setup"});
    }

    private void LaunchProcess(GameWithStatsViewModel game, bool trackPlayTime = false, List<string> arguments = null)
    {
        var process = new Process()
        {
            StartInfo = new ProcessStartInfo(game.GameMetadata.ExecutablePath)
            {
                Arguments = string.Join(" ", arguments ?? new List<string>()),
                WorkingDirectory = game.GameMetadata.InstallDirectory,
                UseShellExecute = true,
            },
            EnableRaisingEvents = true
        };
        if (trackPlayTime)
        {
            process.Exited += (sender, _) => OnGameExited(game, sender as Process);
        }
        else
        {
            process.Exited += (sender, _) => OnSetupExited();
        }
        process.Start();
    }
}