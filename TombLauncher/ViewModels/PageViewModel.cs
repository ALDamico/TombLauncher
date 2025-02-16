using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TombLauncher.Contracts.Progress;

namespace TombLauncher.ViewModels;

public abstract partial class PageViewModel : ViewModelBase
{
    private readonly IProgress<PageBusyState> _progress;
    protected PageViewModel()
    {
        _progress = new Progress<PageBusyState>(state =>
        {
            IsBusy = state.IsBusy;
            BusyMessage = state.BusyMessage;
        });
        SaveCmd = new AsyncRelayCommand(Save, CanSave);
        CancelCmd = new RelayCommand(Cancel, () => IsCancelable);
        TopBarCommands = new ObservableCollection<CommandViewModel>();
    }
    
    [ObservableProperty]private bool _isBusy;
    [ObservableProperty]private string _busyMessage;
    [ObservableProperty] private string _currentFileName;
    [ObservableProperty] private double? _percentageComplete;
    [ObservableProperty] private bool _isCancelable;
    [ObservableProperty] private ObservableCollection<CommandViewModel> _topBarCommands;
    
    public ICommand CancelCmd { get; }

    protected virtual void Cancel()
    {
        
    }

    public void SetBusy(bool isBusy, string busyMessage = null)
    {
        var busyState = new PageBusyState() { IsBusy = isBusy, BusyMessage = busyMessage };
        _progress.Report(busyState);
    }

    public void SetBusy(string busyMessage)
    {
        var busyState = new PageBusyState() { IsBusy = true, BusyMessage = busyMessage };
        _progress.Report(busyState);
    }

    public void ClearBusy()
    {
        _progress.Report(new PageBusyState());
    }

    public ICommand SaveCmd { get; protected set; }
    protected virtual bool CanSave() => false;

    private async Task Save()
    {
        await SaveInner();
    }

    protected virtual Task SaveInner()
    {
        return Task.CompletedTask;
    }
}