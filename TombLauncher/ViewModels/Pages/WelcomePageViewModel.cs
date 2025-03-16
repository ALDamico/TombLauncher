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
    [ObservableProperty] private GameWithStatsViewModel _latestPlayedGame;

    protected override async Task RaiseInitialize()
    {
        _welcomePageService.HandleNotNotifiedCrashes();
        var latestPlayedGame = await _welcomePageService.GetLatestPlayedGame();
        LatestPlayedGame = latestPlayedGame;
        await base.RaiseInitialize();
    }

    [ObservableProperty] private string _changeLogPath;
}