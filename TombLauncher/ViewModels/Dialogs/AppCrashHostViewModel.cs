using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using JamSoft.AvaloniaUI.Dialogs.ViewModels;
using TombLauncher.Core.Dtos;
using TombLauncher.Core.Exceptions;
using TombLauncher.Localization.Extensions;
using TombLauncher.Services;
using TombLauncher.Utils;

namespace TombLauncher.ViewModels.Dialogs;

public class AppCrashHostViewModel : DialogViewModel
{
    public AppCrashHostViewModel(AppCrashHostService appCrashHostService)
    {
        _appCrashHostService = appCrashHostService;
        AcceptCommandText = "Accept".GetLocalizedString();
        CancelCommandText = "Restart application".GetLocalizedString();
        CancelCommand = new RelayCommand(InvokeRestart);
        CopyCmd = new RelayCommand<object>(Copy, CanCopy);
        SaveCmd = new RelayCommand(Save);
    }

    private AppCrashHostService _appCrashHostService;

    private bool CanCopy(object obj)
    {
        return AppUtils.GetClipboard() != null;
    }

    private AppCrashDto _crash;

    public AppCrashDto Crash
    {
        get => _crash;
        set => RaiseAndSetIfChanged(ref _crash, value);
    }

    public ICommand SaveCmd { get; }

    private async void Save()
    {
        await _appCrashHostService.Save(Crash);
    }

    public ICommand CopyCmd { get; }

    private void Copy(object param)
    {
        _appCrashHostService.Copy(param);
    }

    private void InvokeRestart()
    {
        throw new AppRestartRequestedException(Crash.Id);
    }
}