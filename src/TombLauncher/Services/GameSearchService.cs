using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Threading;
using AvaloniaEdit.Utils;
using IconPacks.Avalonia.RemixIcon;
using Microsoft.Extensions.Logging;
using TombLauncher.Contracts.Downloaders;
using TombLauncher.Contracts.Enums;
using TombLauncher.Contracts.Settings;
using TombLauncher.Core.Extensions;
using TombLauncher.Data.Database.Services;
using TombLauncher.Installers.Downloaders;
using TombLauncher.Localization.Extensions;
using TombLauncher.Mappers;
using TombLauncher.ViewModels;
using TombLauncher.ViewModels.Notifications;
using TombLauncher.ViewModels.Pages;

namespace TombLauncher.Services;

public class GameSearchService : IViewService
{
    public GameSearchService(ViewServiceContext viewContext, GameDownloadManager gameDownloadManager,
        GameLinkDataService gameLinkDataService, GameDataService gameDataService,
        NotificationService notificationService,
        ILogger<GameSearchService> logger, ISettingsProvider settingsProvider,
        GameWithStatsService gameWithStatsService,
        GameMetadataMapper gameMapper,
        DownloaderSearchPayloadMapper searchPayloadMapper,
        SearchMapper searchMapper,
        GameSearchResultService gameSearchResultService)
    {
        ViewContext = viewContext;
        _gameDownloadManager = gameDownloadManager;
        _gameLinkDataService = gameLinkDataService;
        _gameDataService = gameDataService;
        _notificationService = notificationService;
        _logger = logger;
        _settingsProvider = settingsProvider;
        _gameWithStatsService = gameWithStatsService;
        _gameMapper = gameMapper;
        _searchPayloadMapper = searchPayloadMapper;
        _searchMapper = searchMapper;
        _gameSearchResultService = gameSearchResultService;
    }

    public ViewServiceContext ViewContext { get; }
    public NavigationManager NavigationManager => ViewContext.NavigationManager;
    private readonly NotificationService _notificationService;
    private readonly ILogger<GameSearchService> _logger;
    private readonly ISettingsProvider _settingsProvider;
    private readonly GameWithStatsService _gameWithStatsService;

    private readonly GameDownloadManager _gameDownloadManager;
    private readonly GameLinkDataService _gameLinkDataService;
    private readonly GameDataService _gameDataService;
    private readonly GameMetadataMapper _gameMapper;
    private readonly DownloaderSearchPayloadMapper _searchPayloadMapper;
    private readonly SearchMapper _searchMapper;
    private readonly GameSearchResultService _gameSearchResultService;

    private Task<List<IMergedGameSearchResultMetadata>> InvokeMerger(GameSearchViewModel target, List<IGameSearchResultMetadata> nextPage)
    {
        var fetchedResults = _searchMapper.ToMergedDtos(target.FetchedResults);
        _gameDownloadManager.Merge(fetchedResults, nextPage);
        return Task.FromResult(fetchedResults);
    }

    public async Task LoadMore(GameSearchViewModel target)
    {
        _logger.LogInformation("Loading more results");
        using (target.BusyScope("LOADING_IN_PROGRESS".GetLocalizedString()))
        {
            var nextPage = target.CurrentPage + 1;
            var nextPageResults = await _gameDownloadManager.GetGames(
                target.LastSearchDownloaders!, target.LastSearchPayload!, nextPage);

            foreach (var failedDownloader in nextPageResults.FailedDownloaders)
            {
                await _notificationService.AddWarningNotificationAsync("FAILED_TO_FETCH".GetLocalizedString(),
                    "FAILED_TO_FETCH_DESCRIPTION".GetLocalizedString(failedDownloader),
                    PackIconRemixIconKind.WifiOffLine);
            }

            var fetchedResults = await InvokeMerger(target, nextPageResults.Results.SelectMany(r => r.Sources).ToList());

            var gamesWithStats = await _gameDataService.GetGamesWithStats();
            var gamesByLinks = await
                _gameLinkDataService.GetGamesByLinksDictionary(LinkType.Download, nextPageResults.Results.SelectMany(g => g.Sources).Select(s => s.DownloadLink!).ToList(), gamesWithStats);
            foreach (var game in target.FetchedResults.Where(r => r.InstalledGame == null))
            {
                if (gamesByLinks.TryGetValue(game.DownloadLink, out var dto))
                {
                    game.InstalledGame = _gameMapper.ToViewModel(dto, _gameWithStatsService);
                }
            }

            var mappedResults = await Task.Run(() => _searchMapper.ToObservableCollection(fetchedResults, _gameSearchResultService));
            var addedCount = 0;
            var updatedCount = 0;

            // Clear previous flags
            foreach (var existing in target.FetchedResults)
            {
                existing.IsNewlyAdded = false;
                existing.IsRecentlyUpdated = false;
            }

            foreach (var result in mappedResults)
            {
                var existingItem =
                    target.FetchedResults.FirstOrDefault(r =>
                        r.Title == result.Title && r.BaseUrl == result.BaseUrl && r.DetailsLink == result.DetailsLink);
                if (existingItem != null)
                {
                    if (existingItem.Sources.Count < result.Sources.Count)
                    {
                        await Dispatcher.UIThread.InvokeAsync(() => existingItem.Sources = result.Sources);
                        existingItem.IsRecentlyUpdated = true;
                        updatedCount++;
                    }
                }
                else
                {
                    result.IsNewlyAdded = true;
                    await Dispatcher.UIThread.InvokeAsync(() => target.FetchedResults.Add(result),
                        DispatcherPriority.Default);
                    addedCount++;
                }
            }

            target.CurrentPage = nextPage;
            target.HasMoreResults = CanLoadMore(target);
            _logger.LogInformation("Loading finished. Has more results: {MoreResults}", target.HasMoreResults);

            // Show feedback
            if (addedCount > 0 || updatedCount > 0)
            {
                var addedText = addedCount > 0 ? "LOAD_MORE_ADDED".GetLocalizedString(addedCount) : "";
                var updatedText = updatedCount > 0 ? "LOAD_MORE_UPDATED".GetLocalizedString(updatedCount) : "";
                var parts = new[] { addedText, updatedText }.Where(s => s.Length > 0);
                var message = string.Join(", ", parts);
                target.LoadMoreFeedback = " (" + message + ")";
                await _notificationService.AddSuccessNotification("Load more", message);
            }
            else
            {
                target.LoadMoreFeedback = string.Empty;
            }
        }
    }

    private bool CanLoadMore(GameSearchViewModel target) => target.CurrentPage < target.MaxTotalPages;

    public async Task Open(GameSearchViewModel target, MultiSourceGameSearchResultMetadataViewModel gameToOpen)
    {
        using (target.BusyScope(string.Empty))
        {
            _logger.LogInformation("Opening game {GameName}", gameToOpen.Title);
            var gameToOpenDto = _searchMapper.ToDto(gameToOpen);

            var details = await _gameDownloadManager.FetchDetails(gameToOpenDto);

            if (details != null)
            {
                var detailsViewModel = _gameMapper.ToViewModel(details);
                var installedGame = await
                    _gameLinkDataService.GetGameByLinks(LinkType.Download, gameToOpen.Sources.Select(s => s.DownloadLink!).ToList());
                if (installedGame != null)
                {
                    detailsViewModel.InstallDirectory = installedGame.InstallDirectory;
                    detailsViewModel.ExecutablePath = installedGame.ExecutablePath;
                    detailsViewModel.IsInstalled = installedGame.IsInstalled;
                    detailsViewModel.SetupExecutable = installedGame.SetupExecutable;
                    detailsViewModel.SetupExecutableArgs = installedGame.SetupExecutableArgs;
                    detailsViewModel.CommunitySetupExecutable = installedGame.CommunitySetupExecutable;
                    detailsViewModel.GameEngine = installedGame.GameEngine;
                    detailsViewModel.Id = installedGame.Id;
                }

                await NavigationManager.NavigateTo<GameDetailsViewModel>(new GameWithStatsViewModel(_gameWithStatsService, detailsViewModel));
                if (NavigationManager.CurrentPage is GameDetailsViewModel currentVm)
                {
                    if (gameToOpen.DownloadLink.IsNotNullOrWhiteSpace())
                    {
                        currentVm.InstallCommand = gameToOpen.InstallCommand;
                    }
                }
            }
        }
    }

    public async Task Search(GameSearchViewModel target)
    {
        using (target.BusyScope("SEARCH_STARTING".GetLocalizedString()))
        {
            _logger.LogInformation("Started search with parameters: {Target}", target);
            var downloaders = _settingsProvider.GetActiveDownloaders();
            if (downloaders.Count == 0)
            {
                await _notificationService.AddErrorNotificationAsync("NO_ACTIVE_DOWNLOADERS".GetLocalizedString(),
                    "NO_ACTIVE_DOWNLOADERS_DESCRIPTION".GetLocalizedString(), PackIconRemixIconKind.WifiOffFill);
                return;
            }
            target.FetchedResults = new ObservableCollection<MultiSourceGameSearchResultMetadataViewModel>();
            try
            {
                var searchPayloadDto = _searchPayloadMapper.ToDto(target.SearchPayload);
                var results = await _gameDownloadManager.GetGames(downloaders, searchPayloadDto, 1);

                if (results.Details != null)
                {
                    var vmToOpen =  _searchMapper.ToViewModel(results.Details, _gameSearchResultService);
                    await Open(target, vmToOpen);
                    return;
                }
                
                foreach (var failedDownloader in results.FailedDownloaders)
                {
                    await _notificationService.AddWarningNotificationAsync("FAILED_TO_FETCH".GetLocalizedString(),
                        "FAILED_TO_FETCH_DESCRIPTION".GetLocalizedString(failedDownloader),
                        PackIconRemixIconKind.WifiOffLine);
                }
                target.LastSearchPayload = searchPayloadDto;
                target.LastSearchDownloaders = downloaders;
                target.CurrentPage = 1;
                target.MaxTotalPages = results.MaxTotalPages ?? 0;
                var mappedGames = _searchMapper.ToViewModels(results.Results, _gameSearchResultService).ToList();
                var downloadLinks = results.Results.SelectMany(g => g.Sources).Select(s => s.DownloadLink!)
                    .Where(s => s.IsNotNullOrWhiteSpace()).ToList();
                var allGamesWithStats = await _gameDataService.GetGamesWithStats();
                var installedGames = await _gameLinkDataService.GetGamesByLinksDictionary(LinkType.Download, downloadLinks, allGamesWithStats);
                foreach (var game in mappedGames)
                {
                    if (game.DownloadLink.IsNullOrWhiteSpace()) 
                        continue;
                    if (installedGames.TryGetValue(game.DownloadLink, out var installedGame))
                    {
                        game.InstalledGame = _gameMapper.ToViewModel(installedGame, _gameWithStatsService);
                    }
                }

                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    target.FetchedResults.Clear();
                    target.FetchedResults.AddRange(mappedGames);
                });
                target.HasMoreResults = CanLoadMore(target);
            }
            catch (OperationCanceledException ex)
            {
                _logger.LogWarning(ex, "Search canceled");
                target.FetchedResults = new ObservableCollection<MultiSourceGameSearchResultMetadataViewModel>();
            }
        }

        if (NavigationManager.CurrentPage != target)
        {
            await _notificationService.AddNotificationAsync(new NotificationViewModel()
            { Content = new StringNotificationViewModel() { Text = "SEARCH_COMPLETED".GetLocalizedString() }, IsDismissable = true });
        }
        _logger.LogInformation("Search completed");
    }

    public void Cancel()
    {
        _gameDownloadManager.CancelCurrentAction();
    }
}