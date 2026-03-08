using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using TombLauncher.Core.Dtos;
using TombLauncher.Services;

namespace TombLauncher.ViewModels.Pages;

public partial class WelcomePageViewModel : PageViewModel
{
    public WelcomePageViewModel(WelcomePageService welcomePageService)
    {
        _welcomePageService = welcomePageService;
    }

    private readonly WelcomePageService _welcomePageService;
    [ObservableProperty] private GameWithStatsViewModel? _latestPlayedGame;
    [ObservableProperty] private QuickStatsDto? _quickStats;
    [ObservableProperty] private bool _showQuickStats = true;

    protected override async Task RaiseInitialize()
    {
        _welcomePageService.HandleNotNotifiedCrashes();
        ShowQuickStats = _welcomePageService.GetShowQuickStats();
        var latestPlayedGame = await _welcomePageService.GetLatestPlayedGame();
        LatestPlayedGame = latestPlayedGame;
        QuickStats = await _welcomePageService.GetQuickStatsAsync();
        await base.RaiseInitialize();
    }

    [ObservableProperty] private string _changeLogPath = string.Empty;
}