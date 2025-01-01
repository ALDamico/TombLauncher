using System.IO;
using System.Text.Json;
using System.Windows.Input;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using JamSoft.AvaloniaUI.Dialogs;
using JamSoft.AvaloniaUI.Dialogs.ViewModels;
using TombLauncher.Core.Dtos;
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
        CopyCmd = new RelayCommand<object>(Copy, CanCopy);
        SaveCmd = new RelayCommand(Save);
        // TODO Hide cancel button when functionality becomes available
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

    public override bool CanCancel() => false;

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
}