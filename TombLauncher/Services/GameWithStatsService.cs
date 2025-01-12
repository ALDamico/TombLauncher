using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.DependencyInjection;
using JamSoft.AvaloniaUI.Dialogs;
using TombLauncher.Contracts.Localization;
using TombLauncher.Core.Extensions;
using TombLauncher.Data.Database.UnitOfWork;
using TombLauncher.Extensions;
using TombLauncher.Localization.Extensions;
using TombLauncher.Navigation;
using TombLauncher.ViewModels;
using TombLauncher.ViewModels.Pages;

namespace TombLauncher.Services;

public class GameWithStatsService : IViewService
{
    public GameWithStatsService()
    {
        GamesUnitOfWork = Ioc.Default.GetRequiredService<GamesUnitOfWork>();
        LocalizationManager = Ioc.Default.GetRequiredService<ILocalizationManager>();
        NavigationManager = Ioc.Default.GetRequiredService<NavigationManager>();
        MessageBoxService = Ioc.Default.GetRequiredService<IMessageBoxService>();
        DialogService = Ioc.Default.GetRequiredService<IDialogService>();
    }
    public GamesUnitOfWork GamesUnitOfWork { get; }
    public ILocalizationManager LocalizationManager { get; }
    public NavigationManager NavigationManager { get; }
    public IMessageBoxService MessageBoxService { get; }
    public IDialogService DialogService { get; }

    public void OpenGame(GameWithStatsViewModel game)
    {
        NavigationManager.NavigateTo(new GameDetailsViewModel(game));
    }

    public async Task OpenGame(int gameId)
    {
        var game = await  GetGameById(gameId);
        OpenGame(game);
    }

    public void PlayGame(GameWithStatsViewModel game)
    {
        var currentPage = NavigationManager.GetCurrentPage();
        currentPage.SetBusy(LocalizationManager.GetLocalizedString("Starting GAMENAME", game.GameMetadata.Title));
        LaunchProcess(game, true);
    }

    public async Task PlayGame(int gameId)
    {
        var gameViewModel = await GetGameById(gameId);
        OpenGame(gameViewModel);
        PlayGame(gameViewModel);
    }

    private async Task<GameWithStatsViewModel> GetGameById(int gameId)
    {
        var game = await Task.Factory.StartNew(() => GamesUnitOfWork.GetGameWithStats(gameId));
        var gameViewModel = new GameWithStatsViewModel()
        {
            GameMetadata = game.GameMetadata.ToViewModel(),
            LastPlayed = game.LastPlayed,
            TotalPlayedTime = game.TotalPlayedTime
        };
        return gameViewModel;
    }

    public bool CanPlayGame(GameWithStatsViewModel game)
    {
        return game.GameMetadata.InstallDirectory.IsNotNullOrWhiteSpace() && game.GameMetadata.ExecutablePath.IsNotNullOrWhiteSpace();
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
        var executable = game.GameMetadata.ExecutablePath;
        if (game.GameMetadata.UniversalLauncherPath.IsNotNullOrWhiteSpace())
        {
            executable = game.GameMetadata.UniversalLauncherPath;
        }
        var process = new Process()
        {
            StartInfo = new ProcessStartInfo(executable)
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
            process.Exited += (_, _) => OnSetupExited();
        }
        process.Start();
    }
}