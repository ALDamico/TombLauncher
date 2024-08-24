using CommunityToolkit.Mvvm.ComponentModel;
using TombLauncher.Navigation;

namespace TombLauncher.ViewModels;

public partial class WelcomePageViewModel : PageViewModel
{
    public WelcomePageViewModel()
    {
        
    }
    [ObservableProperty] private string _changeLogPath;
}