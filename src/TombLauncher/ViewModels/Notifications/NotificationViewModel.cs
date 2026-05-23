using System;
using System.ComponentModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using IconPacks.Avalonia.RemixIcon;
using TombLauncher.Contracts.Enums;

namespace TombLauncher.ViewModels.Notifications;

public partial class NotificationViewModel : ViewModelBase
{
    public NotificationViewModel()
    {
        OpenIcon = PackIconRemixIconKind.FolderOpenLine;
        Timestamp = DateTime.Now;
    }
    [ObservableProperty]
    public partial bool IsDismissable { get; set; }

    [ObservableProperty]
    public partial bool IsCancelable { get; set; }

    [ObservableProperty]
    public partial bool IsOpenable { get; set; }

    [ObservableProperty]
    public partial INotifyPropertyChanged? Content { get; set; }

    [ObservableProperty]
    public partial PackIconRemixIconKind OpenIcon { get; set; }

    [ObservableProperty]
    public partial object? OpenCmdParam { get; set; }

    [ObservableProperty]
    public partial string Title { get; set; } = string.Empty;

    [ObservableProperty]
    public partial NotificationType Type { get; set; }

    [ObservableProperty]
    public partial DateTime Timestamp { get; set; }

    [ObservableProperty]
    public partial bool IsClosing { get; set; }
    public ICommand? DismissCommand { get; set; }
    public ICommand? OpenCommand { get; set; }
    public ICommand? CancelCommand { get; set; }
}