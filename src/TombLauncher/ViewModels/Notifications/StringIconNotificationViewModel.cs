using CommunityToolkit.Mvvm.ComponentModel;
using IconPacks.Avalonia.RemixIcon;

namespace TombLauncher.ViewModels.Notifications;

public partial class StringIconNotificationViewModel : ObservableObject
{
    [ObservableProperty] 
    public partial string Text { get; set; } = string.Empty;
    [ObservableProperty]
    public partial PackIconRemixIconKind Icon { get; set; }
}