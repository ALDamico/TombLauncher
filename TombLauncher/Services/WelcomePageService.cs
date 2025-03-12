using System;
using System.Threading.Tasks;
using AutoMapper;
using CommunityToolkit.Mvvm.DependencyInjection;
using JamSoft.AvaloniaUI.Dialogs;
using TombLauncher.Contracts.Localization;
using TombLauncher.Data.Database.UnitOfWork;
using TombLauncher.Localization;
using TombLauncher.Navigation;
using TombLauncher.ViewModels;
using TombLauncher.ViewModels.Dialogs;

namespace TombLauncher.Services;

public class WelcomePageService : IViewService
{
    public WelcomePageService(AppCrashUnitOfWork appCrashUnitOfWork, ILocalizationManager localizationManager, NavigationManager navigationManager, IMessageBoxService messageBoxService, IDialogService dialogService)
    {
        AppCrashUnitOfWork = appCrashUnitOfWork;
        LocalizationManager = localizationManager;
        NavigationManager = navigationManager;
        MessageBoxService = messageBoxService;
        DialogService = dialogService;
        _gamesUnitOfWork = Ioc.Default.GetRequiredService<GamesUnitOfWork>();
        _mapper = new Mapper(Ioc.Default.GetRequiredService<MapperConfiguration>());
    }
    public AppCrashUnitOfWork AppCrashUnitOfWork { get; }
    public ILocalizationManager LocalizationManager { get; }
    public NavigationManager NavigationManager { get; }
    public IMessageBoxService MessageBoxService { get; }
    public IDialogService DialogService { get; }
    private GamesUnitOfWork _gamesUnitOfWork;
    private IMapper _mapper;

    internal void HandleNotNotifiedCrashes()
    {
        var unnotifiedCrash = AppCrashUnitOfWork.GetNotNotifiedCrashes();
        if (unnotifiedCrash == null) return;
        var appCrashHostService = Ioc.Default.GetRequiredService<AppCrashHostService>();
        var appCrashHostViewModel = new AppCrashHostViewModel(appCrashHostService) { Crash = unnotifiedCrash };

        async void MarkAsNotified(AppCrashHostViewModel model)
        {
            await appCrashHostService.MarkAsNotified(model.Crash);
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