using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using TombLauncher.Services;

namespace TombLauncher.ViewModels.Pages;

public partial class WelcomePageViewModel : PageViewModel
{
    public WelcomePageViewModel()
    {
        _welcomePageService = Ioc.Default.GetRequiredService<WelcomePageService>();
    }

    private readonly WelcomePageService _welcomePageService;

    protected override Task RaiseInitialize()
    {
        _welcomePageService.HandleNotNotifiedCrashes();
        return base.RaiseInitialize();
    }

    [ObservableProperty] private string _changeLogPath;
}