using CommunityToolkit.Mvvm.ComponentModel;

namespace TombLauncher.ViewModels;

public partial class NotificationViewModel : ViewModelBase
{
    [ObservableProperty] private bool _isDismissable;
    [ObservableProperty] private ViewModelBase _content;
}