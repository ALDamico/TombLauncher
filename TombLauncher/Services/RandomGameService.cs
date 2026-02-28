using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using TombLauncher.Contracts.Downloaders;
using TombLauncher.Contracts.Enums;
using TombLauncher.Core.Extensions;
using TombLauncher.Core.Navigation;
using TombLauncher.Data.Database.UnitOfWork;
using TombLauncher.Installers.Downloaders;
using TombLauncher.Localization.Extensions;
using TombLauncher.ViewModels;
using TombLauncher.ViewModels.Pages;

namespace TombLauncher.Services;

public class RandomGameService
{
    public RandomGameService(
        ISettingsProvider settingsProvider,
        GamesUnitOfWork gamesUnitOfWork,
        GameDownloadManager gameDownloadManager,
        NavigationManager navigationManager,
        MapperConfiguration mapperConfiguration,
        GameSearchResultService gameSearchResultService,
        GameWithStatsService gameWithStatsService)
    {
        _settingsProvider = settingsProvider;
        _gamesUnitOfWork = gamesUnitOfWork;
        _gameDownloadManager = gameDownloadManager;
        _navigationManager = navigationManager;
        _mapper = mapperConfiguration.CreateMapper();
        _gameSearchResultService = gameSearchResultService;
        _gameWithStatsService = gameWithStatsService;
    }

    private ISettingsProvider _settingsProvider;
    private GamesUnitOfWork _gamesUnitOfWork;
    private GameDownloadManager _gameDownloadManager;
    private NavigationManager _navigationManager;
    private IMapper _mapper;
    private GameSearchResultService _gameSearchResultService;
    private GameWithStatsService _gameWithStatsService;

    public async Task PickRandomGame(RandomGameViewModel target)
    {
        target.AttemptsExpired = false;
        target.SetBusy("Picking a random game for you...".GetLocalizedString());
        target.MaxRetries = _settingsProvider.GetApplicationSettings().RandomGameMaxRerolls;
        var maxRerolls = target.MaxRetries;
        for (var i = 0; i < maxRerolls; i++)
        {
            if (i > 0)
            {
                target.SetBusy(true,
                    "Picking a random game for you... (Attempt NR of TOT)".GetLocalizedString(i + 1, maxRerolls));
            }

            var downloaders = _settingsProvider.GetActiveDownloaders();
            var downloaderToUse = downloaders.PickOneAtRandom();

            // Initialization call to discover the total number of pages
            await downloaderToUse.GetGames(new DownloaderSearchPayload(), CancellationToken.None);

            var random = new Random();
            var pageToFetch = random.Next(downloaderToUse.TotalPages.GetValueOrDefault() - 1);


            var gamePage = await downloaderToUse.FetchPage(pageToFetch, CancellationToken.None);
            var pickedGame = gamePage.PickOneAtRandom();

            var allGamesResult = await _gameDownloadManager.GetGames(new DownloaderSearchPayload()
            { LevelName = pickedGame.Title })
                ;

            var details = await _gameDownloadManager.FetchDetails(pickedGame);

            var candidate = allGamesResult.FirstOrDefault();
            if (candidate == null)
            {
                continue;
            }

            var installedGame = await _gamesUnitOfWork.GetGameByLinks(LinkType.Download,
                candidate.Sources.Select(s => s.DownloadLink).ToList());
            if (installedGame == null)
            {
                var mapped = _mapper.Map<MultiSourceGameSearchResultMetadataViewModel>(candidate);

                var gameWithStats = new GameWithStatsViewModel(_gameWithStatsService)
                { GameMetadata = _mapper.Map<GameMetadataViewModel>(details) };

                await _navigationManager.NavigateTo<GameDetailsViewModel>(gameWithStats);

                if (_navigationManager.CurrentPage is GameDetailsViewModel vm)
                {
                    vm.InstallCmd = new AsyncRelayCommand(async () =>
                    {
                        vm.SetBusy(true, $"Installing {mapped.Title}"); // SetBusy via INavigationTarget is on PageViewModel base? Yes.
                        await _gameSearchResultService.Install(
                            mapped);

                        vm.Game.GameMetadata.InstallDirectory = mapped.InstalledGame.GameMetadata.InstallDirectory;
                        vm.Game.GameMetadata.ExecutablePath = mapped.InstalledGame.GameMetadata.ExecutablePath;
                        vm.Game.GameMetadata.Id = mapped.InstalledGame.GameMetadata.Id;
                        vm.Game.RaiseCanExecuteChanged(vm.Game.PlayCmd);
                        vm.Game.RaiseCanExecuteChanged(vm.Game.LaunchSetupCmd);
                        vm.Game.RaiseCanExecuteChanged(vm.Game.OpenCmd);
                        vm.InstallCmd = null;
                        vm.SetBusy(false);
                    }, () => mapped.InstalledGame == null);
                }
                return;
            }
        }

        target.AttemptsExpired = true;
    }
}