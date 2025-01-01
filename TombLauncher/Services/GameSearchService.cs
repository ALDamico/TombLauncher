using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using AutoMapper;
using Avalonia.Threading;
using AvaloniaEdit.Utils;
using CommunityToolkit.Mvvm.DependencyInjection;
using JamSoft.AvaloniaUI.Dialogs;
using TombLauncher.Contracts.Downloaders;
using TombLauncher.Contracts.Enums;
using TombLauncher.Contracts.Localization;
using TombLauncher.Core.Dtos;
using TombLauncher.Core.Extensions;
using TombLauncher.Data.Database.UnitOfWork;
using TombLauncher.Extensions;
using TombLauncher.Installers.Downloaders;
using TombLauncher.Localization.Extensions;
using TombLauncher.Navigation;
using TombLauncher.Utils;
using TombLauncher.ViewModels;
using TombLauncher.ViewModels.Pages;

namespace TombLauncher.Services;

public class GameSearchService : IViewService
{
    public GameSearchService(GameDownloadManager gameDownloadManager, GamesUnitOfWork gamesUnitOfWork,
        ILocalizationManager localizationManager, NavigationManager navigationManager,
        IMessageBoxService messageBoxService, IDialogService dialogService, MapperConfiguration mapperConfiguration,
        NotificationService notificationService, GameListService gameListService)
    {
        GameDownloadManager = gameDownloadManager;
        GamesUnitOfWork = gamesUnitOfWork;
        LocalizationManager = localizationManager;
        NavigationManager = navigationManager;
        MessageBoxService = messageBoxService;
        DialogService = dialogService;
        _mapper = mapperConfiguration.CreateMapper();
        _notificationService = notificationService;
        _gameListService = gameListService;
    }

    private NotificationService _notificationService;
    private GameListService _gameListService;

    public GameDownloadManager GameDownloadManager { get; }
    public GamesUnitOfWork GamesUnitOfWork { get; }
    public ILocalizationManager LocalizationManager { get; }
    public NavigationManager NavigationManager { get; }
    public IMessageBoxService MessageBoxService { get; }
    public IDialogService DialogService { get; }
    private IMapper _mapper;

    private Task<List<IMultiSourceSearchResultMetadata>> InvokeMerger(GameSearchViewModel target, List<IGameSearchResultMetadata> nextPage)
    {
        var fetchedResults = _mapper.Map<List<IMultiSourceSearchResultMetadata>>(target.FetchedResults);
        GameDownloadManager.Merge(fetchedResults, nextPage);
        return Task.FromResult(fetchedResults);
    }

    public async Task LoadMore(GameSearchViewModel target)
    {
        target.SetBusy("Loading in progress".GetLocalizedString());
        var nextPage = await GameDownloadManager.FetchNextPage();

        var fetchedResults = await InvokeMerger(target, nextPage);
        
        var gamesByLinks =
            GamesUnitOfWork.GetGamesByLinksDictionary(LinkType.Download, nextPage.Select(p => p.DownloadLink).ToList());
        foreach (var game in Enumerable.Where(target.FetchedResults, r => r.InstalledGame == null))
        {
            if (gamesByLinks.TryGetValue(game.DownloadLink, out var dto))
            {
                game.InstalledGame = _mapper.Map<GameWithStatsViewModel>(dto);
            }
        }

        var mappedResults = await new TaskFactory().StartNew(() => 
            _mapper.Map<List<MultiSourceGameSearchResultMetadataViewModel>>(fetchedResults));
        foreach (var result in mappedResults)
        {
            var existingItem =
                target.FetchedResults.FirstOrDefault(r =>
                    r.Title == result.Title && r.BaseUrl == result.BaseUrl && r.DetailsLink == result.DetailsLink);
            if (existingItem != null)
            {
                await Dispatcher.UIThread.InvokeAsync(() => existingItem.Sources = result.Sources);
            }
            else
            {
                Console.WriteLine($"Adding new item {result.Title}");
                await Dispatcher.UIThread.InvokeAsync(() => target.FetchedResults.Add(result),
                    DispatcherPriority.Default);
            }

            await Task.Delay(1);
        }

        await new TaskFactory().StartNew(() => { return Task.CompletedTask; });


        target.HasMoreResults = CanLoadMore();
        target.ClearBusy();
    }

    public bool CanLoadMore() => GameDownloadManager.HasMoreResults();

    public async Task Open(GameSearchViewModel target, MultiSourceGameSearchResultMetadataViewModel gameToOpen)
    {
        target.SetBusy(true);
        var gameToOpenDto = _mapper.Map<GameSearchResultMetadataDto>(gameToOpen);

        var details = await GameDownloadManager.FetchDetails(gameToOpenDto);
        if (details != null)
        {
            var detailsViewModel = _mapper.Map<GameMetadataViewModel>(details);

            var gameDetailsService = Ioc.Default.GetRequiredService<GameDetailsService>();
            var gameWithStatsService = Ioc.Default.GetRequiredService<GameWithStatsService>();
            var settingsService = Ioc.Default.GetRequiredService<SettingsService>();
            var vm = new GameDetailsViewModel(gameDetailsService,
                new GameWithStatsViewModel(gameWithStatsService) { GameMetadata = detailsViewModel }, settingsService);

            if (details.TitlePic is { Length: > 0 } && gameToOpen.TitlePic == null)
            {
                gameToOpen.TitlePic = gameToOpenDto.TitlePic;
                // TODO Reimplement this
                //gameToOpen.TitlePic =  ImageUtils.ToBitmap(details.TitlePic);
            }

            target.ClearBusy();
            NavigationManager.NavigateTo(vm);
            return;
        }

        target.ClearBusy();
    }

    public async Task Search(GameSearchViewModel target)
    {
        target.SetBusy("Search starting...".GetLocalizedString());
        var settingsService = Ioc.Default.GetRequiredService<SettingsService>();
        var downloaders = settingsService.GetActiveDownloaders();
        GameDownloadManager.Downloaders.Clear();
        GameDownloadManager.Downloaders.AddRange(downloaders);
        target.FetchedResults = new ObservableCollection<MultiSourceGameSearchResultMetadataViewModel>();
        try
        {
            var games = await GameDownloadManager.GetGames(target.SearchPayload.ToDto());
            var mappedGames = _mapper.Map<List<MultiSourceGameSearchResultMetadataViewModel>>(games);
            var downloadLinks = games.SelectMany(g => g.Sources).Select(s => s.DownloadLink) /*TODO Remove this*/
                .Where(s => s.IsNotNullOrWhiteSpace()).ToList();
            var installedGames = GamesUnitOfWork.GetGamesByLinksDictionary(LinkType.Download, downloadLinks);
            foreach (var game in mappedGames)
            {
                if (game.DownloadLink == null) continue;
                if (installedGames.TryGetValue(game.DownloadLink, out var installedGame))
                {
                    game.InstalledGame = _mapper.Map<GameWithStatsViewModel>(installedGame);
                }
            }

            var observableCollection = mappedGames.ToObservableCollection();

            Dispatcher.UIThread.Invoke(() =>
            {
                target.FetchedResults.Clear();
                target.FetchedResults.AddRange(observableCollection);
            });
            target.HasMoreResults = GameDownloadManager.HasMoreResults();
        }
        catch (OperationCanceledException)
        {
            target.FetchedResults = new ObservableCollection<MultiSourceGameSearchResultMetadataViewModel>();
        }

        target.ClearBusy();

        if (NavigationManager.GetCurrentPage() != target)
        {
            await _notificationService.AddNotification(new NotificationViewModel()
                { Content = new StringNotificationViewModel() { Text = "Search completed" }, IsDismissable = true });
        }
    }

    public void Cancel()
    {
        GameDownloadManager.CancelCurrentAction();
    }
}