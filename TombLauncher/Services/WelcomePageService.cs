using System.Threading.Tasks;
using AutoMapper;
using CommunityToolkit.Mvvm.DependencyInjection;
using JamSoft.AvaloniaUI.Dialogs;
using TombLauncher.Contracts.Localization;
using TombLauncher.Core.Navigation;
using TombLauncher.Data.Database.UnitOfWork;
using TombLauncher.ViewModels;
using TombLauncher.ViewModels.Dialogs;

namespace TombLauncher.Services;

public class WelcomePageService : IViewService
{
    public WelcomePageService(ViewServiceContext viewContext, AppCrashUnitOfWork appCrashUnitOfWork, GamesUnitOfWork gamesUnitOfWork, AppCrashHostService appCrashHostService)
    {
        ViewContext = viewContext;
        AppCrashUnitOfWork = appCrashUnitOfWork;
        _gamesUnitOfWork = gamesUnitOfWork;
        _appCrashHostService = appCrashHostService;
    }
    public ViewServiceContext ViewContext { get; }
    public AppCrashUnitOfWork AppCrashUnitOfWork { get; }
    public ILocalizationManager LocalizationManager => ViewContext.LocalizationManager;
    public NavigationManager NavigationManager => ViewContext.NavigationManager;
    public IMessageBoxService MessageBoxService => ViewContext.MessageBoxService;
    public IDialogService DialogService => ViewContext.DialogService;
    private IMapper _mapper => ViewContext.Mapper;
    private GamesUnitOfWork _gamesUnitOfWork;
    private AppCrashHostService _appCrashHostService;

    internal void HandleNotNotifiedCrashes()
    {
        var unnotifiedCrash = AppCrashUnitOfWork.GetNotNotifiedCrashes();
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
        var latestPlayedGame = _gamesUnitOfWork.GetLatestPlayedGame();
        var viewModel = _mapper.Map<GameWithStatsViewModel>(latestPlayedGame);
        return await Task.FromResult(viewModel);
    }
}