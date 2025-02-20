using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.Input;
using JamSoft.AvaloniaUI.Dialogs.Events;
using JamSoft.AvaloniaUI.Dialogs.MsgBox;
using JamSoft.AvaloniaUI.Dialogs.ViewModels;
using TombLauncher.Localization.Extensions;
using TombLauncher.Utils;

namespace TombLauncher.ViewModels.MessageBoxes;

public class UrlOpenErrorMessageBox : ViewModelBase, IMsgBoxViewModel
{
    public UrlOpenErrorMessageBox()
    {
        CheckBoxText = null;
        AcceptCommand = new AsyncRelayCommand(CopyUrlToClipboard);
        NoCommand = new RelayCommand(Close);
        CancelCommand = new RelayCommand(Close);
        MsgBoxImage = MsgBoxImage.Error;
    }
    public ICommand AcceptCommand { get; set; }

    private Task CopyUrlToClipboard()
    {
        var task = AppUtils.SetClipboardTextAsync(TargetUrl);
        OnRequestCloseDialog(new RequestCloseDialogEventArgs(true));
        return task;
    }
    public ICommand CancelCommand { get; set; }

    private void Close()
    {
        OnRequestCloseDialog(new RequestCloseDialogEventArgs(false));
    }
    public event EventHandler<RequestCloseDialogEventArgs> RequestCloseDialog;
    public bool CheckBoxResult { get; set; } = false;
    public bool ShowCheckBox => CheckBoxText != null;
    public string CheckBoxText { get; set; } = null;
    public bool HasIcon => false;
    public MsgBoxImage MsgBoxImage { get; set; }
    public MsgBoxButtonResult Result { get; set; }
    public MsgBoxButton Buttons { get; set; } = MsgBoxButton.YesNo;
    public WindowStartupLocation WindowStartupLocation { get; set; } = WindowStartupLocation.CenterOwner;
    public bool Topmost { get; set; }
    public Bitmap Icon { get; set; }
    public string AcceptCommandText { get; set; } = "Yes".GetLocalizedString();
    public ICommand NoCommand { get; set; }
    public string CancelCommandText { get; set; }
    public string Message { get; set; }
    public string MsgBoxTitle { get; set; }
    public string NoCommandText { get; set; } = "No".GetLocalizedString();
    public bool ShowNoButton { get; set; } = true;
    public bool ShowYesButton { get; set; } = true;
    public bool ShowOkButton { get; set; } = false;
    public bool ShowCancelButton { get; set; } = false;
    public string TargetUrl { get; set; }


    protected virtual void OnRequestCloseDialog(RequestCloseDialogEventArgs e)
    {
        RequestCloseDialog?.Invoke(this, e);
    }
}