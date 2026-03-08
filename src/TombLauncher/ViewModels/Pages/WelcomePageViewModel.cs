using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
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
    [ObservableProperty] private bool _showQuickActions = true;

    protected override async Task RaiseInitialize()
    {
        _welcomePageService.HandleNotNotifiedCrashes();
        ShowQuickStats = _welcomePageService.GetShowQuickStats();
        ShowQuickActions = _welcomePageService.GetShowQuickActions();
        var latestPlayedGame = await _welcomePageService.GetLatestPlayedGame();
        LatestPlayedGame = latestPlayedGame;
        QuickStats = await _welcomePageService.GetQuickStatsAsync();
        await base.RaiseInitialize();
    }

    [RelayCommand]
    private async Task AddGame() => await _welcomePageService.NavigateToNewGame();

    [RelayCommand]
    private async Task SearchOnline() => await _welcomePageService.NavigateToSearch();

    [RelayCommand]
    private async Task RandomGame() => await _welcomePageService.NavigateToRandomGame();

    [ObservableProperty] private string _changeLogPath = string.Empty;
}