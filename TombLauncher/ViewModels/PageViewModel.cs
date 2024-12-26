using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace TombLauncher.ViewModels;

public abstract partial class PageViewModel : ViewModelBase
{
    protected PageViewModel()
    {
        SaveCmd = new AsyncRelayCommand(Save, CanSave);
        CancelCmd = new RelayCommand(Cancel, () => IsCancelable);
        TopBarCommands = new ObservableCollection<CommandViewModel>();
    }

    public bool IsBusy
    {
        get => _isBusy;
        protected set
        {
            _isBusy = value;
            OnPropertyChanged();
        }
    }
    
    private bool _isBusy;
    private string _busyMessage;

    public string BusyMessage
    {
        get => _busyMessage;
        protected set
        {
            _busyMessage = value;
            OnPropertyChanged();
        }
    }
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
        IsBusy = isBusy;
        if (busyMessage != null)
        {
            BusyMessage = busyMessage;
        }
    }

    public void SetBusy(string busyMessage)
    {
        IsBusy = true;
        BusyMessage = busyMessage;
    }

    public void ClearBusy()
    {
        IsBusy = false;
        BusyMessage = null;
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