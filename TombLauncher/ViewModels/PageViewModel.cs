using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace TombLauncher.ViewModels;

public partial class PageViewModel : ViewModelBase
{
    public PageViewModel()
    {
        SaveCmd = new RelayCommand(Save, CanSave);
    }
    [ObservableProperty] private bool _isBusy;

    public ICommand SaveCmd { get; protected set; }
    protected virtual bool CanSave() => false;

    private async void Save()
    {
        SaveInner();
    }

    protected virtual void SaveInner()
    {
    }
}