using System.Threading.Tasks;
using AutoMapper;
using TombLauncher.Configuration;
using TombLauncher.Core.Dtos;
using JamSoft.AvaloniaUI.Dialogs;
using TombLauncher.Contracts.Localization;
using TombLauncher.Data.Database.Services;
using TombLauncher.ViewModels;
using TombLauncher.ViewModels.Dialogs;

namespace TombLauncher.Services;

public class WelcomePageService : IViewService
{
    public WelcomePageService(ViewServiceContext viewContext, AppCrashDataService appCrashDataService, GameDataService gameDataService, AppCrashHostService appCrashHostService, IAppConfigurationWrapper appConfiguration)
    {
        ViewContext = viewContext;
        _appCrashDataService = appCrashDataService;
        _gameDataService = gameDataService;
        _appCrashHostService = appCrashHostService;
        _appConfiguration = appConfiguration;
    }
    public ViewServiceContext ViewContext { get; }
    private readonly AppCrashDataService _appCrashDataService;
    private readonly IAppConfigurationWrapper _appConfiguration;
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

    internal bool GetShowQuickStats() => _appConfiguration.ShowQuickStats.GetValueOrDefault(true);
}