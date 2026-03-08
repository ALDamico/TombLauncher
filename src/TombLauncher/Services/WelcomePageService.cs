using System.Threading.Tasks;
using AutoMapper;
using TombLauncher.Core.Dtos;
using JamSoft.AvaloniaUI.Dialogs;
using TombLauncher.Contracts.Localization;
using TombLauncher.Data.Database.Services;
using TombLauncher.ViewModels;
using TombLauncher.ViewModels.Dialogs;
using TombLauncher.ViewModels.Pages;

namespace TombLauncher.Services;

public class WelcomePageService : IViewService
{
    public WelcomePageService(ViewServiceContext viewContext, AppCrashDataService appCrashDataService, GameDataService gameDataService, AppCrashHostService appCrashHostService, SettingsPageService settingsPageService)
    {
        ViewContext = viewContext;
        _appCrashDataService = appCrashDataService;
        _gameDataService = gameDataService;
        _appCrashHostService = appCrashHostService;
        _settingsPageService = settingsPageService;
    }
    public ViewServiceContext ViewContext { get; }
    private readonly AppCrashDataService _appCrashDataService;
    private readonly SettingsPageService _settingsPageService;
    public ILocalizationManager LocalizationManager => ViewContext.LocalizationManager;
    public NavigationManager NavigationManager => ViewContext.NavigationManager;
    public IMessageBoxService MessageBoxService => ViewContext.MessageBoxService;
    public IDialogService DialogService => ViewContext.DialogService;
    private IMapper Mapper => ViewContext.Mapper;
    private readonly GameDataService _gameDataService;
    private readonly AppCrashHostService _appCrashHostService;

    internal void HandleNotNotifiedCrashes()
    {
        var unnotifiedCrash = _appCrashDataService.GetNotNotifiedCrashes();
        if (unnotifiedCrash == null) return;
        var appCrashHostViewModel = new AppCrashHostViewModel(_appCrashHostService) { Crash = unnotifiedCrash };

        async void MarkAsNotified(AppCrashHostViewModel model)
        {
            await _appCrashHostService.MarkAsNotified(model.Crash);
        }

        DialogService.ShowDialog(appCrashHostViewModel, MarkAsNotified);
    }

    internal async Task<GameWithStatsViewModel> GetLatestPlayedGame()
    {
        var latestPlayedGame = _gameDataService.GetLatestPlayedGame();
        var viewModel = Mapper.Map<GameWithStatsViewModel>(latestPlayedGame);
        return await Task.FromResult(viewModel);
    }

    internal async Task<QuickStatsDto> GetQuickStatsAsync()
    {
        return await _gameDataService.GetQuickStatsAsync();
    }

    internal bool GetShowQuickStats() => _settingsPageService.GetShowQuickStats();
    internal bool GetShowQuickActions() => _settingsPageService.GetShowQuickActions();

    internal async Task NavigateToNewGame() => await NavigationManager.NavigateTo<NewGameViewModel>();
    internal async Task NavigateToSearch() => await NavigationManager.NavigateTo<GameSearchViewModel>();
    internal async Task NavigateToRandomGame() => await NavigationManager.NavigateToRoot<RandomGameViewModel>();
}