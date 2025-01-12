using CommunityToolkit.Mvvm.ComponentModel;

namespace TombLauncher.ViewModels.Notifications;

public partial class StringNotificationViewModel : ViewModelBase
{
    [ObservableProperty] private string _text;
}