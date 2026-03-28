using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TombLauncher.Contracts.Navigation;
using TombLauncher.Contracts.Progress;
using TombLauncher.Localization.Extensions;

namespace TombLauncher.ViewModels;

public abstract partial class PageViewModel : ViewModelBase, INavigationTarget, INavigableViewModel
{
    private readonly IProgress<PageBusyState> _progress;

    public virtual Task OnNavigatedTo(object parameter)
    {
        return Task.CompletedTask;
    }

    public virtual Task OnNavigatingFrom()
    {
        return Task.CompletedTask;
    }
    protected PageViewModel()
    {
        _progress = new Progress<PageBusyState>(state =>
        {
            IsBusy = state.IsBusy;
            BusyMessage = state.BusyMessage ?? string.Empty;
        });
        SaveCmd = new AsyncRelayCommand(Save, CanSave);
        CancelCmd = new RelayCommand(Cancel, () => IsCancelable);
        TopBarCommands = new ObservableCollection<ITopBarCommand>();
    }

    [ObservableProperty] private bool _isBusy;
    [ObservableProperty] private string _busyMessage = string.Empty;
    [ObservableProperty] private string _currentFileName = string.Empty;
    [ObservableProperty] private double? _percentageComplete;
    [ObservableProperty] private bool _isCancelable;
    [ObservableProperty] private ObservableCollection<ITopBarCommand> _topBarCommands;

    public ICommand CancelCmd { get; }

    protected virtual void Cancel()
    {

    }

    private void SetBusy(bool isBusy, string? busyMessage = null)
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

    public IDisposable BusyScope(string busyMessage)
    {
        SetBusy(true, busyMessage);
        return new BusyDisposable(this);
    }

    private sealed class BusyDisposable(INavigationTarget vm) : IDisposable
    {
        public void Dispose() => vm.ClearBusy();
    }

    public ICommand SaveCmd { get; protected set; }
    protected virtual bool CanSave() => false;

    private async Task Save()
    {
        using (BusyScope("INSTALLING".GetLocalizedString()))
        {
            await SaveInner();
        }
    }

    protected virtual Task SaveInner()
    {
        return Task.CompletedTask;
    }
}