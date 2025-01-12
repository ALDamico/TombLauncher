using CommunityToolkit.Mvvm.ComponentModel;
using Material.Icons;

namespace TombLauncher.ViewModels.Notifications;

public partial class StringIconNotificationViewModel : ObservableObject
{
    [ObservableProperty] private string _text;
    [ObservableProperty] private MaterialIconKind _icon;
}