using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using JamSoft.AvaloniaUI.Dialogs;
using TombLauncher.Database.UnitOfWork;
using TombLauncher.Localization;

namespace TombLauncher.ViewModels;

public partial class WelcomePageViewModel : PageViewModel
{
    public WelcomePageViewModel(LocalizationManager localizationManager, AppCrashUnitOfWork appCrashUoW,
        IDialogService dialogService) : base(localizationManager, dialogService:dialogService)
    {
        _appCrashUoW = appCrashUoW;
        InitCmd = new RelayCommand(InitializeInner);
    }

    private void InitializeInner()
    {
        var unnotifiedCrash = _appCrashUoW.GetNotNotifiedCrashes();
        if (unnotifiedCrash == null) return;
        DialogService.ShowDialog(new AppCrashHostViewModel(DialogService) { Crash = unnotifiedCrash },
            model => { _appCrashUoW.MarkAsNotified(model.Crash.Id); });
    }

    private readonly AppCrashUnitOfWork _appCrashUoW;

    [ObservableProperty] private string _changeLogPath;
}