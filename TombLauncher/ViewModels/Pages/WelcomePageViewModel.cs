using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using TombLauncher.Services;

namespace TombLauncher.ViewModels.Pages;

public partial class WelcomePageViewModel : PageViewModel
{
    public WelcomePageViewModel()
    {
        _welcomePageService = Ioc.Default.GetRequiredService<WelcomePageService>();
        Initialize += InitializeInner;
    }

    private readonly WelcomePageService _welcomePageService;

    private void InitializeInner()
    {
        _welcomePageService.HandleNotNotifiedCrashes();
    }

    [ObservableProperty] private string _changeLogPath;
}