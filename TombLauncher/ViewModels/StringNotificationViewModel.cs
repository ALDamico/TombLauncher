using CommunityToolkit.Mvvm.ComponentModel;

namespace TombLauncher.ViewModels;

public partial class StringNotificationViewModel : ViewModelBase
{
    [ObservableProperty] private string _text;
}