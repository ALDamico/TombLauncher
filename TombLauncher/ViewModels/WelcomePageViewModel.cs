using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using JamSoft.AvaloniaUI.Dialogs;
using TombLauncher.Database.UnitOfWork;
using TombLauncher.Localization;

namespace TombLauncher.ViewModels;

public partial class WelcomePageViewModel : PageViewModel
{
    public WelcomePageViewModel(LocalizationManager localizationManager, AppCrashUnitOfWork appCrashUoW,
        IDialogService dialogService) : base(localizationManager)
    {
        _appCrashUoW = appCrashUoW;
        _dialogService = dialogService;
        InitCmd = new RelayCommand(InitializeInner);
    }

    private void InitializeInner()
    {
        var unnotifiedCrash = _appCrashUoW.GetNotNotifiedCrashes();
        if (unnotifiedCrash == null) return;
        _dialogService.ShowDialog(new AppCrashHostViewModel() { Crash = unnotifiedCrash },
            model => { _appCrashUoW.MarkAsNotified(model.Crash.Id); });
    }

    private readonly AppCrashUnitOfWork _appCrashUoW;
    private readonly IDialogService _dialogService;

    [ObservableProperty] private string _changeLogPath;
}