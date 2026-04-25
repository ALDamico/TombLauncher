using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using JamSoft.AvaloniaUI.Dialogs.ViewModels;
using TombLauncher.Core.Dtos;
using TombLauncher.Core.Exceptions;
using TombLauncher.Localization.Extensions;
using TombLauncher.Services;

namespace TombLauncher.ViewModels.Dialogs;

public class AppCrashHostViewModel : DialogViewModel
{
    public AppCrashHostViewModel(AppCrashHostService appCrashHostService)
    {
        _appCrashHostService = appCrashHostService;
        AcceptCommandText = "ACCEPT".GetLocalizedString();
        CancelCommandText = "RESTART_APPLICATION".GetLocalizedString();
        CancelCommand = new RelayCommand(InvokeRestart);
        CopyCommand = new RelayCommand<object>(Copy, CanCopy);
        SaveCommand = new RelayCommand(Save);
    }

    private readonly AppCrashHostService _appCrashHostService;

    private bool CanCopy(object? obj)
    {
        return true;
    }

    public AppCrashDto Crash
    {
        get;
        set => RaiseAndSetIfChanged(ref field, value);
    } = null!;

    public ICommand SaveCommand { get; }

    private async void Save()
    {
        await _appCrashHostService.Save(Crash);
    }

    public ICommand CopyCommand { get; }

    private void Copy(object? param)
    {
        if (param != null)
        {
            _appCrashHostService.Copy(param);
        }
    }

    private void InvokeRestart()
    {
        throw new AppRestartRequestedException(Crash.Id);
    }
}