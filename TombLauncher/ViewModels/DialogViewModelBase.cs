using System;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using JamSoft.AvaloniaUI.Dialogs.Events;
using JamSoft.AvaloniaUI.Dialogs.ViewModels;

namespace TombLauncher.ViewModels;

public abstract class DialogViewModelBase : ViewModelBase, IDialogViewModel
{
    protected DialogViewModelBase()
    {
        AcceptCommand = new RelayCommand(Accept, CanAccept);
        CancelCommand = new RelayCommand(Cancel, CanCancel);
    }
    public ICommand AcceptCommand { get; set; }
    public ICommand CancelCommand { get; set; }
    public event EventHandler<RequestCloseDialogEventArgs> RequestCloseDialog;

    protected virtual void Accept()
    {
        
        RequestCloseDialog?.Invoke(this, new RequestCloseDialogEventArgs(true));
    }
    protected abstract void Cancel();
    public bool CanAccept()
    {
        return CanAcceptInner();
    }

    protected virtual bool CanAcceptInner()
    {
        return false;
    }

    public bool CanCancel()
    {
        return CanCancelInner();
    }

    protected virtual bool CanCancelInner()
    {
        return false;
    }

    public string AcceptCommandText { get; set; }
    public string CancelCommandText { get; set; }
}