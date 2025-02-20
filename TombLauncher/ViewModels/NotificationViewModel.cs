using System.ComponentModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using Material.Icons;

namespace TombLauncher.ViewModels;

public partial class NotificationViewModel : ViewModelBase
{
    public NotificationViewModel()
    {
        OpenIcon = MaterialIconKind.Folder;
    }
    [ObservableProperty] private bool _isDismissable;
    [ObservableProperty] private bool _isCancelable;
    [ObservableProperty] private bool _isOpenable;
    [ObservableProperty] private INotifyPropertyChanged _content;
    [ObservableProperty] private MaterialIconKind _openIcon;
    [ObservableProperty] private object _openCmdParam;
    [ObservableProperty] private string _title;
    
    public ICommand DismissCmd { get; set; }
    public ICommand OpenCommand { get; set; }
    public ICommand CancelCommand { get; set; }
}