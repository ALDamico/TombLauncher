using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Avalonia.Threading;
using AvaloniaEdit.Utils;
using JamSoft.AvaloniaUI.Dialogs;
using Microsoft.Extensions.Logging;
using TombLauncher.Contracts.Downloaders;
using TombLauncher.Contracts.Enums;
using TombLauncher.Contracts.Localization;
using TombLauncher.Core.Dtos;
using TombLauncher.Core.Extensions;
using TombLauncher.Data.Database.Services;
using TombLauncher.Installers.Downloaders;
using TombLauncher.Localization.Extensions;
using TombLauncher.ViewModels;
using TombLauncher.ViewModels.Pages;
using StringNotificationViewModel = TombLauncher.ViewModels.Notifications.StringNotificationViewModel;

namespace TombLauncher.Services;

public class GameSearchService : IViewService
{
    public GameSearchService(ViewServiceContext viewContext, GameDownloadManager gameDownloadManager,
        GameLinkDataService gameLinkDataService, GameDataService gameDataService,
        NotificationService notificationService, GameListService gameListService,
        ILogger<GameSearchService> logger, ISettingsProvider settingsProvider,
        GameWithStatsService gameWithStatsService)
    {
        ViewContext = viewContext;
        _gameDownloadManager = gameDownloadManager;
        _gameLinkDataService = gameLinkDataService;
        _gameDataService = gameDataService;
        _notificationService = notificationService;
        _gameListService = gameListService;
        _logger = logger;
        _settingsProvider = settingsProvider;
        _gameWithStatsService = gameWithStatsService;
    }

    public ViewServiceContext ViewContext { get; }
    public ILocalizationManager LocalizationManager => ViewContext.LocalizationManager;
    public NavigationManager NavigationManager => ViewContext.NavigationManager;
    public IMessageBoxService MessageBoxService => ViewContext.MessageBoxService;
    public IDialogService DialogService => ViewContext.DialogService;
    private IMapper Mapper => ViewContext.Mapper;
    private readonly NotificationService _notificationService;
    private readonly GameListService _gameListService;
    private readonly ILogger<GameSearchService> _logger;
    private readonly ISettingsProvider _settingsProvider;
    private readonly GameWithStatsService _gameWithStatsService;

    private readonly GameDownloadManager _gameDownloadManager;
    private readonly GameLinkDataService _gameLinkDataService;
    private readonly GameDataService _gameDataService;

    private Task<List<IMergedGameSearchResultMetadata>> InvokeMerger(GameSearchViewModel target, List<IGameSearchResultMetadata> nextPage)
    {
        var fetchedResults = Mapper.Map<List<IMergedGameSearchResultMetadata>>(target.FetchedResults);
        _gameDownloadManager.Merge(fetchedResults, nextPage);
        return Task.FromResult(fetchedResults);
    }

    public async Task LoadMore(GameSearchViewModel target)
    {
        _logger.LogInformation("Loading more results");
        target.SetBusy("LOADING_IN_PROGRESS".GetLocalizedString());
        var nextPage = await _gameDownloadManager.FetchNextPage();

        var fetchedResults = await InvokeMerger(target, nextPage);

        var gamesWithStats = await _gameDataService.GetGamesWithStats();
        var gamesByLinks = await
            _gameLinkDataService.GetGamesByLinksDictionary(LinkType.Download, nextPage.Select(p => p.DownloadLink).ToList(), gamesWithStats);
        foreach (var game in Enumerable.Where(target.FetchedResults, r => r.InstalledGame == null))
        {
            if (gamesByLinks.TryGetValue(game.DownloadLink, out var dto))
            {
                game.InstalledGame = Mapper.Map<GameWithStatsViewModel>(dto);
            }
        }

        var mappedResults = await new TaskFactory().StartNew(() =>
            Mapper.Map<List<MultiSourceGameSearchResultMetadataViewModel>>(fetchedResults));
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

        target.HasMoreResults = CanLoadMore();
        _logger.LogInformation("Loading finished. Has more results: {MoreResults}", target.HasMoreResults);
        target.ClearBusy();

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

    public bool CanLoadMore() => _gameDownloadManager.HasMoreResults();

    public async Task Open(GameSearchViewModel target, MultiSourceGameSearchResultMetadataViewModel gameToOpen)
    {
        target.SetBusy(true);
        _logger.LogInformation("Opening game {GameName}", gameToOpen.Title);
        var gameToOpenDto = Mapper.Map<GameSearchResultMetadataDto>(gameToOpen);

        var details = await _gameDownloadManager.FetchDetails(gameToOpenDto);

        if (details != null)
        {
            var detailsViewModel = Mapper.Map<GameMetadataViewModel>(details);
            var installedGame = await
                _gameLinkDataService.GetGameByLinks(LinkType.Download, gameToOpen.Sources.Select(s => s.DownloadLink).ToList());
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



            // Logic moved to after navigation block


            target.ClearBusy();

            // Note: passing the whole VM might check if GameDetailsViewModel handles it correctly
            // GameDetailsViewModel expects GameWithStatsViewModel. 
            // vm above was created manually. 
            // We should pass "new GameWithStatsViewModel(detailsViewModel)" as parameter.
            // And we need to handle "vm.InstallCmd = ..." after navigation? 
            // Or pass a composite parameter?
            // "vm" is local here. We are navigating to a NEW GameDetailsViewModel via DI.
            // The InstallCmd assignment on the NEW VM needs to happen.
            // OnNavigatedTo receives the parameter.
            // If I pass GameWithStatsViewModel, the InstallCmd is NOT on it.
            // The InstallCmd is on gameToOpen? No, gameToOpen.InstallCmd.
            // The logic assigns vm.InstallCmd = gameToOpen.InstallCmd.
            // I should pass a "GameDetailsParameter" or just "GameWithStatsViewModel" and set InstallCmd differently?
            // Or maybe handle InstallCmd in GameDetailsViewModel differently?
            // "gameToOpen.InstallCmd" comes from where?
            // It seems "gameToOpen" is MultiSourceGameSearchResultMetadataViewModel.

            // Let's create a DTO or just pass the GameWithStatsViewModel and handling InstallCmd logic is tricky if VM is created by DI.
            // Can I retrieve the VM after navigation?
            // NavigationManager.CurrentPage is updated.

            await NavigationManager.NavigateTo<GameDetailsViewModel>(new GameWithStatsViewModel(_gameWithStatsService, detailsViewModel));
            if (NavigationManager.CurrentPage is GameDetailsViewModel currentVm)
            {
                if (gameToOpen.DownloadLink.IsNotNullOrWhiteSpace())
                {
                    currentVm.InstallCmd = gameToOpen.InstallCmd;
                }
            }
            return;
        }

        target.ClearBusy();
    }

    public async Task Search(GameSearchViewModel target)
    {
        target.SetBusy("SEARCH_STARTING".GetLocalizedString());
        _logger.LogInformation("Started search with parameters: {Target}", target);
        var downloaders = _settingsProvider.GetActiveDownloaders();
        _gameDownloadManager.Downloaders.Clear();
        _gameDownloadManager.Downloaders.AddRange(downloaders);
        target.FetchedResults = new ObservableCollection<MultiSourceGameSearchResultMetadataViewModel>();
        try
        {
            var searchPayloadDto = Mapper.Map<DownloaderSearchPayload>(target.SearchPayload);
            var games = await _gameDownloadManager.GetGames(searchPayloadDto);
            var mappedGames = Mapper.Map<List<MultiSourceGameSearchResultMetadataViewModel>>(games);
            var downloadLinks = games.SelectMany(g => g.Sources).Select(s => s.DownloadLink) /*TODO Remove this*/
                .Where(s => s.IsNotNullOrWhiteSpace()).ToList();
            var allGamesWithStats = await _gameDataService.GetGamesWithStats();
            var installedGames = await _gameLinkDataService.GetGamesByLinksDictionary(LinkType.Download, downloadLinks, allGamesWithStats);
            foreach (var game in mappedGames)
            {
                if (game.DownloadLink == null) continue;
                if (installedGames.TryGetValue(game.DownloadLink, out var installedGame))
                {
                    game.InstalledGame = Mapper.Map<GameWithStatsViewModel>(installedGame);
                }
            }

            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                target.FetchedResults.Clear();
                target.FetchedResults.AddRange(mappedGames);
            });
            target.HasMoreResults = _gameDownloadManager.HasMoreResults();
        }
        catch (OperationCanceledException ex)
        {
            _logger.LogWarning(ex, "Search canceled");
            target.FetchedResults = new ObservableCollection<MultiSourceGameSearchResultMetadataViewModel>();
        }

        target.ClearBusy();

        if (NavigationManager.CurrentPage != target)
        {
            await _notificationService.AddNotificationAsync(new NotificationViewModel()
            { Content = new StringNotificationViewModel() { Text = "Search completed" }, IsDismissable = true });
        }
        _logger.LogInformation("Search completed");
    }

    public void Cancel()
    {
        _gameDownloadManager.CancelCurrentAction();
    }
}