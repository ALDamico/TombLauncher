using System.ComponentModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using JamSoft.AvaloniaUI.Dialogs;
using TombLauncher.Localization;
using TombLauncher.Navigation;

namespace TombLauncher.ViewModels;

public abstract partial class PageViewModel : ViewModelBase
{
    public PageViewModel(LocalizationManager localizationManager, IMessageBoxService messageBoxService = null, IDialogService dialogService = null)
    {
        LocalizationManager = localizationManager;
        MessageBoxService = messageBoxService;
        DialogService = dialogService;
        SaveCmd = new RelayCommand(Save, CanSave);
        CancelCmd = new RelayCommand(Cancel, () => IsCancelable);
    }
    [ObservableProperty] private bool _isBusy;
    [ObservableProperty] private string _busyMessage;
    [ObservableProperty] private string _currentFileName;
    [ObservableProperty] private double? _percentageComplete;
    [ObservableProperty] private bool _isCancelable;
    
    public ICommand CancelCmd { get; }

    protected virtual void Cancel()
    {
        
    }

    protected void SetBusy(bool isBusy, string busyMessage = null)
    {
        IsBusy = isBusy;
        if (busyMessage != null)
        {
            BusyMessage = busyMessage;
        }
    }

    protected void ClearBusy()
    {
        IsBusy = false;
        BusyMessage = null;
    }

    public RelayCommand SaveCmd { get; protected set; }
    protected virtual bool CanSave() => false;

    private async void Save()
    {
        SaveInner();
    }

    protected virtual void SaveInner()
    {
    }
    protected LocalizationManager LocalizationManager { get; private set; }
    protected IMessageBoxService MessageBoxService { get; private set; }
    public IDialogService DialogService { get; private set; }
}