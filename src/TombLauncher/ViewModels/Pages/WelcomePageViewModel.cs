using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
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
    [ObservableProperty] private bool _showRecentlyPlayed = true;
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasRecentlyPlayedGames))]
    [NotifyCanExecuteChangedFor(nameof(PreviousRecentGameCommand))]
    [NotifyCanExecuteChangedFor(nameof(NextRecentGameCommand))]
    private ObservableCollection<GameWithStatsViewModel> _recentlyPlayedGames = new();
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(PreviousRecentGameCommand))]
    [NotifyCanExecuteChangedFor(nameof(NextRecentGameCommand))]
    private int _recentlyPlayedIndex;
    public bool HasRecentlyPlayedGames => RecentlyPlayedGames.Count > 0;
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasRecentlyPlayedGames))]
    [NotifyCanExecuteChangedFor(nameof(PreviousRecentGameCommand))]
    [NotifyCanExecuteChangedFor(nameof(NextRecentGameCommand))]
    private List<int> _paginationIndices = new();

    protected override async Task RaiseInitialize()
    {
        _welcomePageService.HandleNotNotifiedCrashes();
        ShowQuickStats = _welcomePageService.GetShowQuickStats();
        ShowQuickActions = _welcomePageService.GetShowQuickActions();
        ShowRecentlyPlayed = _welcomePageService.GetShowRecentlyPlayed();
        var latestPlayedGame = await _welcomePageService.GetLatestPlayedGame();
        LatestPlayedGame = latestPlayedGame;
        QuickStats = await _welcomePageService.GetQuickStatsAsync();
        RecentlyPlayedGames = new ObservableCollection<GameWithStatsViewModel>(
            _welcomePageService.GetRecentlyPlayedGames());
        PaginationIndices = Enumerable.Range(0, RecentlyPlayedGames.Count).ToList();
        RecentlyPlayedIndex = 0; // Force notification for initial dot selection
        await base.RaiseInitialize();
    }

    [RelayCommand]
    private async Task AddGame() => await _welcomePageService.NavigateToNewGame();

    [RelayCommand]
    private async Task SearchOnline() => await _welcomePageService.NavigateToSearch();

    [RelayCommand]
    private async Task RandomGame() => await _welcomePageService.NavigateToRandomGame();

    [RelayCommand(CanExecute = nameof(CanGoPrevious))]
    private void PreviousRecentGame() => RecentlyPlayedIndex--;

    [RelayCommand(CanExecute = nameof(CanGoNext))]
    private void NextRecentGame() => RecentlyPlayedIndex++;

    private bool CanGoPrevious() => RecentlyPlayedIndex > 0;
    private bool CanGoNext() => RecentlyPlayedIndex < RecentlyPlayedGames.Count - 1;

    [ObservableProperty] private string _changeLogPath = string.Empty;
}