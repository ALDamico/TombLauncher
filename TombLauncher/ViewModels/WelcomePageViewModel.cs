using CommunityToolkit.Mvvm.ComponentModel;
using TombLauncher.Localization;
using TombLauncher.Navigation;

namespace TombLauncher.ViewModels;

public partial class WelcomePageViewModel : PageViewModel
{
    public WelcomePageViewModel(LocalizationManager localizationManager) : base(localizationManager)
    {
        
    }
    [ObservableProperty] private string _changeLogPath;
}