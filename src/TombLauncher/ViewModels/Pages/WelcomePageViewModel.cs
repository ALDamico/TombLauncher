using System;
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
    [ObservableProperty] private bool _showFavourites = true;
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

    // Favourites carousel
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasFavouriteGames))]
    [NotifyCanExecuteChangedFor(nameof(PreviousFavouriteCommand))]
    [NotifyCanExecuteChangedFor(nameof(NextFavouriteCommand))]
    private ObservableCollection<GameWithStatsViewModel> _favouriteGames = new();
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(PreviousFavouriteCommand))]
    [NotifyCanExecuteChangedFor(nameof(NextFavouriteCommand))]
    private int _favouriteIndex;
    public bool HasFavouriteGames => FavouriteGames.Count > 0;
    [ObservableProperty] private List<int> _favouritePaginationIndices = new();

    [ObservableProperty] private bool _showRandomSuggestion = true;
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasRandomSuggestion))]
    private MultiSourceGameSearchResultMetadataViewModel? _randomSuggestion;
    [ObservableProperty] private bool _isLoadingRandomSuggestion;
    [ObservableProperty] private bool _randomSuggestionFailed;
    public bool HasRandomSuggestion => RandomSuggestion != null;

    protected override async Task RaiseInitialize()
    {
        _welcomePageService.HandleNotNotifiedCrashes();
        ShowQuickStats = _welcomePageService.GetShowQuickStats();
        ShowQuickActions = _welcomePageService.GetShowQuickActions();
        ShowRecentlyPlayed = _welcomePageService.GetShowRecentlyPlayed();
        ShowFavourites = _welcomePageService.GetShowFavourites();
        ShowRandomSuggestion = _welcomePageService.GetShowRandomSuggestion();
        var latestPlayedGame = await _welcomePageService.GetLatestPlayedGame();
        LatestPlayedGame = latestPlayedGame;
        QuickStats = await _welcomePageService.GetQuickStatsAsync();
        RecentlyPlayedGames = new ObservableCollection<GameWithStatsViewModel>(
            _welcomePageService.GetRecentlyPlayedGames(_welcomePageService.GetRecentlyPlayedCount()));
        PaginationIndices = Enumerable.Range(0, RecentlyPlayedGames.Count).ToList();
        RecentlyPlayedIndex = 0;
        FavouriteGames = new ObservableCollection<GameWithStatsViewModel>(
            _welcomePageService.GetFavouriteGames(_welcomePageService.GetFavouritesCount()));
        FavouritePaginationIndices = Enumerable.Range(0, FavouriteGames.Count).ToList();
        FavouriteIndex = 0;
        if (ShowRandomSuggestion)
        {
            _ = LoadRandomSuggestionAsync();
        }
        await base.RaiseInitialize();
    }

    [RelayCommand]
    private async Task AddGame() => await _welcomePageService.NavigateToNewGame();

    [RelayCommand]
    private async Task SearchOnline() => await _welcomePageService.NavigateToSearch();

    [RelayCommand(CanExecute = nameof(CanGoPrevious))]
    private void PreviousRecentGame() => RecentlyPlayedIndex--;

    [RelayCommand(CanExecute = nameof(CanGoNext))]
    private void NextRecentGame() => RecentlyPlayedIndex++;

    private bool CanGoPrevious() => RecentlyPlayedIndex > 0;
    private bool CanGoNext() => RecentlyPlayedIndex < RecentlyPlayedGames.Count - 1;

    [RelayCommand(CanExecute = nameof(CanGoPreviousFavourite))]
    private void PreviousFavourite() => FavouriteIndex--;

    [RelayCommand(CanExecute = nameof(CanGoNextFavourite))]
    private void NextFavourite() => FavouriteIndex++;

    private bool CanGoPreviousFavourite() => FavouriteIndex > 0;
    private bool CanGoNextFavourite() => FavouriteIndex < FavouriteGames.Count - 1;

    public event Action? ScrollToRandomSuggestionRequested;

    [RelayCommand]
    private async Task ShuffleRandomSuggestion()
    {
        ScrollToRandomSuggestionRequested?.Invoke();
        await LoadRandomSuggestionAsync();
    }

    [RelayCommand]
    private async Task OpenRandomSuggestion()
    {
        if (RandomSuggestion != null)
        {
            await _welcomePageService.OpenRandomGameSuggestionAsync(RandomSuggestion);
        }
    }

    private async Task LoadRandomSuggestionAsync()
    {
        IsLoadingRandomSuggestion = true;
        RandomSuggestionFailed = false;
        RandomSuggestion = null;
        try
        {
            RandomSuggestion = await _welcomePageService.FetchRandomGameSuggestionAsync();
            if (RandomSuggestion == null)
            {
                RandomSuggestionFailed = true;
            }
        }
        catch
        {
            RandomSuggestionFailed = true;
        }
        finally
        {
            IsLoadingRandomSuggestion = false;
        }
    }

    [ObservableProperty] private string _changeLogPath = string.Empty;
}