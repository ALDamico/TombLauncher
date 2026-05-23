using CommunityToolkit.Mvvm.ComponentModel;

namespace TombLauncher.ViewModels.Notifications;

public partial class StringNotificationViewModel : ViewModelBase
{
    [ObservableProperty]
    public partial string Text { get; set; } = string.Empty;
}