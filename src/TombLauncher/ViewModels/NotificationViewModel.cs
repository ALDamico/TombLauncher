using System;
using System.ComponentModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using IconPacks.Avalonia.RemixIcon;
using TombLauncher.Contracts.Enums;

namespace TombLauncher.ViewModels;

public partial class NotificationViewModel : ViewModelBase
{
    public NotificationViewModel()
    {
        OpenIcon = PackIconRemixIconKind.FolderOpenLine;
        Timestamp = DateTime.Now;
    }
    [ObservableProperty] private bool _isDismissable;
    [ObservableProperty] private bool _isCancelable;
    [ObservableProperty] private bool _isOpenable;
    [ObservableProperty] private INotifyPropertyChanged? _content;
    [ObservableProperty] private PackIconRemixIconKind _openIcon;
    [ObservableProperty] private object? _openCmdParam;
    [ObservableProperty] private string _title = string.Empty;
    [ObservableProperty] private NotificationType _type;
    [ObservableProperty] private DateTime _timestamp;
    [ObservableProperty] private bool _isClosing;

    public ICommand? DismissCommand { get; set; }
    public ICommand? OpenCommand { get; set; }
    public ICommand? CancelCommand { get; set; }
}