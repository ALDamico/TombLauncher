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
    [ObservableProperty] public partial GameWithStatsViewModel? LatestPlayedGame { get; set; }
    [ObservableProperty]
    public partial QuickStatsDto? QuickStats { get; set; }

    [ObservableProperty]
    public partial bool ShowQuickStats { get; set; } = true;

    [ObservableProperty]
    public partial bool ShowQuickActions { get; set; } = true;

    [ObservableProperty]
    public partial bool ShowRecentlyPlayed { get; set; } = true;

    [ObservableProperty]
    public partial bool ShowFavourites { get; set; } = true;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(PreviousRecentGameCommand))]
    [NotifyCanExecuteChangedFor(nameof(NextRecentGameCommand))]
    public partial ObservableCollection<GameWithStatsViewModel> RecentlyPlayedGames { get; set; } = [];

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(PreviousRecentGameCommand))]
    [NotifyCanExecuteChangedFor(nameof(NextRecentGameCommand))]
    public partial int RecentlyPlayedIndex { get; set; }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(PreviousRecentGameCommand))]
    [NotifyCanExecuteChangedFor(nameof(NextRecentGameCommand))]
    public partial List<int> PaginationIndices { get; set; } = [];

    // Favourites carousel
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(PreviousFavouriteCommand))]
    [NotifyCanExecuteChangedFor(nameof(NextFavouriteCommand))]
    public partial ObservableCollection<GameWithStatsViewModel> FavouriteGames { get; set; } = new();

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(PreviousFavouriteCommand))]
    [NotifyCanExecuteChangedFor(nameof(NextFavouriteCommand))]
    private int _favouriteIndex;
    [ObservableProperty]
    public partial List<int> FavouritePaginationIndices { get; set; } = new();

    [ObservableProperty]
    public partial bool ShowRandomSuggestion { get; set; } = true;

    [ObservableProperty]
    public partial MultiSourceGameSearchResultMetadataViewModel? RandomSuggestion { get; set; }

    [ObservableProperty]
    public partial bool IsLoadingRandomSuggestion { get; set; }

    [ObservableProperty]
    public partial bool RandomSuggestionFailed { get; set; }

    [ObservableProperty]
    public partial string? ChangeLogMarkdown { get; set; }

    [ObservableProperty]
    public partial bool IsLoadingChangelog { get; set; } = true;

    [ObservableProperty]
    public partial bool ChangelogLoadFailed { get; set; }

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
        _ = LoadChangelogAsync();
        await base.RaiseInitialize();
    }

    [RelayCommand]
    private async Task AddGame() => await _welcomePageService.NavigateToNewGame();

    [RelayCommand]
    private async Task SearchOnline() => await _welcomePageService.NavigateToSearch();

    [RelayCommand]
    private async Task RetryLoadChangelog() => await LoadChangelogAsync();


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

    private async Task LoadChangelogAsync()
    {
        IsLoadingChangelog = true;
        ChangelogLoadFailed = false;
        try
        {
            ChangeLogMarkdown = await _welcomePageService.FetchChangelogAsync();
            if (ChangeLogMarkdown == null)
            {
                ChangelogLoadFailed = true;
            }
        }
        catch
        {
            ChangelogLoadFailed = true;
        }
        finally
        {
            IsLoadingChangelog = false;
        }
    }
}