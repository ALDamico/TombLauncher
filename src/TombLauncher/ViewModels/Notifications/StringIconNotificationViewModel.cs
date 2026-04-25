using CommunityToolkit.Mvvm.ComponentModel;
using IconPacks.Avalonia.RemixIcon;

namespace TombLauncher.ViewModels.Notifications;

public partial class StringIconNotificationViewModel : ObservableObject
{
    [ObservableProperty] private string _text = string.Empty;
    [ObservableProperty] private PackIconRemixIconKind _icon;
}