using CommunityToolkit.Mvvm.ComponentModel;
using TombLauncher.Services;

namespace TombLauncher.ViewModels.Pages;

public partial class WelcomePageViewModel : PageViewModel
{
    public WelcomePageViewModel(WelcomePageService welcomePageService)
    {
        _welcomePageService = welcomePageService;
        Initialize += InitializeInner;
    }

    private WelcomePageService _welcomePageService;

    private void InitializeInner()
    {
        _welcomePageService.HandleNotNotifiedCrashes();
    }

    [ObservableProperty] private string _changeLogPath;
}