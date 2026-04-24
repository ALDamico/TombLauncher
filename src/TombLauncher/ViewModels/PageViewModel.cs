using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TombLauncher.Contracts.Navigation;
using TombLauncher.Contracts.Progress;
using TombLauncher.Core.Navigation;

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
        TopBarCommands = new ObservableCollection<ITopBarCommand>();
    }

    [ObservableProperty] private bool _isBusy;
    [ObservableProperty] private string _busyMessage = string.Empty;
    [ObservableProperty] private string _currentFileName = string.Empty;
    [ObservableProperty] private double? _percentageComplete;
    [ObservableProperty] private bool _isCancelable;
    [ObservableProperty] private ObservableCollection<ITopBarCommand> _topBarCommands;

    [RelayCommand(CanExecute = nameof(IsCancelable))]
    protected virtual void Cancel()
    {

    }

    public void SetBusy(bool isBusy, string? busyMessage = null)
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

    private sealed class BusyDisposable(PageViewModel vm) : IDisposable
    {
        public void Dispose() => vm.ClearBusy();
    }

    protected virtual bool CanSave() => false;

    [RelayCommand(AllowConcurrentExecutions = false, CanExecute = nameof(CanSave))]
    private async Task Save()
    {
        await SaveInner();
    }

    protected virtual Task SaveInner()
    {
        return Task.CompletedTask;
    }
}