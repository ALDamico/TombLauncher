using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using TombLauncher.Configuration;
using TombLauncher.Contracts.Downloaders;
using TombLauncher.Core.Dtos;
using TombLauncher.Core.Extensions;
using TombLauncher.Contracts.Localization;
using TombLauncher.Data.Database.Services;
using TombLauncher.Installers.Downloaders;
using TombLauncher.ViewModels;
using TombLauncher.ViewModels.Dialogs;
using TombLauncher.ViewModels.Pages;

namespace TombLauncher.Services;

public class WelcomePageService : IViewService
{
    public WelcomePageService(ViewServiceContext viewContext, AppCrashDataService appCrashDataService, GameDataService gameDataService, AppCrashHostService appCrashHostService, IAppConfiguration appConfiguration, ISettingsProvider settingsProvider, GameDownloadManager gameDownloadManager, GameWithStatsService gameWithStatsService, GitHubReleaseService gitHubReleaseService)
    {
        ViewContext = viewContext;
        _appCrashDataService = appCrashDataService;
        _gameDataService = gameDataService;
        _appCrashHostService = appCrashHostService;
        _appConfiguration = appConfiguration;
        _settingsProvider = settingsProvider;
        _gameDownloadManager = gameDownloadManager;
        _gameWithStatsService = gameWithStatsService;
        _gitHubReleaseService = gitHubReleaseService;
    }
    public ViewServiceContext ViewContext { get; }
    private readonly AppCrashDataService _appCrashDataService;
    private readonly IAppConfiguration _appConfiguration;
    private readonly ISettingsProvider _settingsProvider;
    private readonly GameDownloadManager _gameDownloadManager;
    private readonly GameWithStatsService _gameWithStatsService;
    private readonly GitHubReleaseService _gitHubReleaseService;
    private IMapper Mapper => ViewContext.Mapper;
    public ILocalizationManager LocalizationManager => ViewContext.LocalizationManager;
    public NavigationManager NavigationManager => ViewContext.NavigationManager;
    private readonly GameDataService _gameDataService;
    private readonly AppCrashHostService _appCrashHostService;

    internal void HandleNotNotifiedCrashes()
    {
        var unnotifiedCrash = _appCrashDataService.GetNotNotifiedCrashes();
        if (unnotifiedCrash == null) return;
        var appCrashHostViewModel = new AppCrashHostViewModel(_appCrashHostService) { Crash = unnotifiedCrash };

        async void MarkAsNotified(AppCrashHostViewModel model)
        {
            await _appCrashHostService.MarkAsNotified(model.Crash);
        }

        ViewContext.PopupService.ShowDialog(appCrashHostViewModel, MarkAsNotified);
    }

    internal async Task<GameWithStatsViewModel> GetLatestPlayedGame()
    {
        var latestPlayedGame = _gameDataService.GetLatestPlayedGame();
        var viewModel = Mapper.Map<GameWithStatsViewModel>(latestPlayedGame);
        return await Task.FromResult(viewModel);
    }

    internal async Task<QuickStatsDto> GetQuickStatsAsync()
    {
        return await _gameDataService.GetQuickStatsAsync();
    }

    internal bool GetShowQuickStats() => _appConfiguration.WelcomePage.ShowQuickStats.GetValueOrDefault(true);
    internal bool GetShowQuickActions() => _appConfiguration.WelcomePage.ShowQuickActions.GetValueOrDefault(true);
    internal bool GetShowRecentlyPlayed() => _appConfiguration.WelcomePage.ShowRecentlyPlayed.GetValueOrDefault(true);
    internal bool GetShowFavourites() => _appConfiguration.WelcomePage.ShowFavourites.GetValueOrDefault(true);
    internal int GetRecentlyPlayedCount() => _appConfiguration.WelcomePage.RecentlyPlayedCount.GetValueOrDefault(5);
    internal int GetFavouritesCount() => _appConfiguration.WelcomePage.FavouritesCount.GetValueOrDefault(5);
    internal bool GetShowRandomSuggestion() => _appConfiguration.WelcomePage.ShowRandomSuggestion.GetValueOrDefault(true);

    internal List<GameWithStatsViewModel> GetRecentlyPlayedGames(int count = 5)
    {
        var dtos = _gameDataService.GetRecentlyPlayedGames(count);
        return dtos.Select(Mapper.Map<GameWithStatsViewModel>).ToList();
    }

    internal List<GameWithStatsViewModel> GetFavouriteGames(int count = 5)
    {
        var dtos = _gameDataService.GetFavouriteGames(count);
        return dtos.Select(Mapper.Map<GameWithStatsViewModel>).ToList();
    }

    internal async Task<MultiSourceGameSearchResultMetadataViewModel?> FetchRandomGameSuggestionAsync()
    {
        var maxAttempts = _settingsProvider.GetApplicationSettings().RandomGameMaxRerolls;
        for (var i = 0; i < maxAttempts; i++)
        {
            try
            {
                var downloaders = _settingsProvider.GetActiveDownloaders();
                var downloaderToUse = downloaders.PickOneAtRandom();
                var emptyPayload = new DownloaderSearchPayload();

                // Fetch page 1 to discover TotalPages
                var firstPage = await downloaderToUse.Search.GetGames(emptyPayload, 1, CancellationToken.None);

                var random = new Random();
                var pageToFetch = random.Next(firstPage.TotalPages.GetValueOrDefault(1));
                var gamePage = pageToFetch > 1
                    ? await downloaderToUse.Search.GetGames(emptyPayload, pageToFetch, CancellationToken.None)
                    : firstPage;
                var pickedGame = gamePage.Results.ToList().PickOneAtRandom();

                var searchResult = await _gameDownloadManager.GetGames(
                    downloaders, new DownloaderSearchPayload() { LevelName = pickedGame.Title }, 1);
                var allGamesResult = searchResult.Results;

                var candidate = allGamesResult.FirstOrDefault();
                if (candidate != null)
                {
                    return Mapper.Map<MultiSourceGameSearchResultMetadataViewModel>(candidate);
                }
            }
            catch
            {
                // Retry on next attempt
            }
        }

        return null;
    }

    internal async Task OpenRandomGameSuggestionAsync(MultiSourceGameSearchResultMetadataViewModel gameToOpen)
    {
        var gameToOpenDto = Mapper.Map<GameSearchResultMetadataDto>(gameToOpen);
        var details = await _gameDownloadManager.FetchDetails(gameToOpenDto);
        if (details != null)
        {
            var detailsViewModel = Mapper.Map<GameMetadataViewModel>(details);
            await NavigationManager.NavigateTo<GameDetailsViewModel>(
                new GameWithStatsViewModel(_gameWithStatsService, detailsViewModel));
            if (NavigationManager.CurrentPage is GameDetailsViewModel currentVm)
            {
                if (gameToOpen.DownloadLink.IsNotNullOrWhiteSpace() && gameToOpen.DownloadLink.Trim().Length > 0)
                {
                    currentVm.InstallCmd = gameToOpen.InstallCommand;
                }
            }
        }
    }

    internal async Task NavigateToNewGame() => await NavigationManager.NavigateTo<NewGameViewModel>();
    internal async Task NavigateToSearch() => await NavigationManager.NavigateTo<GameSearchViewModel>();

    internal Task<string?> FetchChangelogAsync() => _gitHubReleaseService.GetChangelogMarkdownAsync();
}