using System;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace TombLauncher.ViewModels;

public abstract class ViewModelBase : ObservableObject
{
    protected ViewModelBase()
    {
        InitCmd = new AsyncRelayCommand(RaiseInitialize);
    }
    
    public ICommand InitCmd { get; set; }

    protected virtual Task RaiseInitialize()
    {
        return Task.CompletedTask;
    }
    
    internal void RaiseCanExecuteChanged<T>(ICommand command)
    {
        if (command is RelayCommand<T> relayCommand)
            relayCommand.NotifyCanExecuteChanged();
        else if (command is AsyncRelayCommand<T> asyncRelayCommand)
            asyncRelayCommand.NotifyCanExecuteChanged();
    }

    internal void RaiseCanExecuteChanged(ICommand command)
    {
        if (command is RelayCommand relayCommand)
        {
            relayCommand.NotifyCanExecuteChanged();
        }
        else if (command is AsyncRelayCommand asyncRelayCommand)
        {
            asyncRelayCommand.NotifyCanExecuteChanged();
        }
    }
}