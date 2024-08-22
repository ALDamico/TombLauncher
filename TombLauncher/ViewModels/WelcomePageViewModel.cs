using CommunityToolkit.Mvvm.ComponentModel;

namespace TombLauncher.ViewModels;

public partial class WelcomePageViewModel : PageViewModel
{
    [ObservableProperty] private string _changeLogPath;
}