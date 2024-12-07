using CommunityToolkit.Mvvm.DependencyInjection;
using JamSoft.AvaloniaUI.Dialogs;
using TombLauncher.Data.Database.UnitOfWork;
using TombLauncher.Localization;
using TombLauncher.Navigation;
using TombLauncher.ViewModels.Dialogs;

namespace TombLauncher.Services;

public class WelcomePageService : IViewService
{
    public WelcomePageService(AppCrashUnitOfWork appCrashUnitOfWork, LocalizationManager localizationManager, NavigationManager navigationManager, IMessageBoxService messageBoxService, IDialogService dialogService)
    {
        AppCrashUnitOfWork = appCrashUnitOfWork;
        LocalizationManager = localizationManager;
        NavigationManager = navigationManager;
        MessageBoxService = messageBoxService;
        DialogService = dialogService;
    }
    public AppCrashUnitOfWork AppCrashUnitOfWork { get; }
    public LocalizationManager LocalizationManager { get; }
    public NavigationManager NavigationManager { get; }
    public IMessageBoxService MessageBoxService { get; }
    public IDialogService DialogService { get; }

    internal void HandleNotNotifiedCrashes()
    {
        var unnotifiedCrash = AppCrashUnitOfWork.GetNotNotifiedCrashes();
        if (unnotifiedCrash == null) return;
        var appCrashHostService = Ioc.Default.GetRequiredService<AppCrashHostService>();
        DialogService.ShowDialog(new AppCrashHostViewModel(appCrashHostService) { Crash = unnotifiedCrash },
            model => { appCrashHostService.MarkAsNotified(model.Crash); });
    }
}