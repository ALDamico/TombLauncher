using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using CommunityToolkit.Mvvm.Input;
using TombLauncher.Contracts.Downloaders;
using TombLauncher.Contracts.Enums;
using TombLauncher.Core.Extensions;
using TombLauncher.Data.Database.Services;
using TombLauncher.Installers.Downloaders;
using TombLauncher.Localization.Extensions;
using TombLauncher.ViewModels;
using TombLauncher.ViewModels.Pages;

namespace TombLauncher.Services;

public class RandomGameService
{
    public RandomGameService(
        ISettingsProvider settingsProvider,
        GameLinkDataService gameLinkDataService,
        GameDownloadManager gameDownloadManager,
        NavigationManager navigationManager,
        MapperConfiguration mapperConfiguration,
        GameSearchResultService gameSearchResultService,
        GameWithStatsService gameWithStatsService)
    {
        _settingsProvider = settingsProvider;
        _gameLinkDataService = gameLinkDataService;
        _gameDownloadManager = gameDownloadManager;
        _navigationManager = navigationManager;
        _mapper = mapperConfiguration.CreateMapper();
        _gameSearchResultService = gameSearchResultService;
        _gameWithStatsService = gameWithStatsService;
    }

    private readonly ISettingsProvider _settingsProvider;
    private readonly GameLinkDataService _gameLinkDataService;
    private readonly GameDownloadManager _gameDownloadManager;
    private readonly NavigationManager _navigationManager;
    private readonly IMapper _mapper;
    private readonly GameSearchResultService _gameSearchResultService;
    private readonly GameWithStatsService _gameWithStatsService;

    public async Task PickRandomGame(RandomGameViewModel target)
    {
        target.AttemptsExpired = false;
        target.SetBusy("PICKING_A_RANDOM_GAME_FOR_YOU".GetLocalizedString());
        target.MaxRetries = _settingsProvider.GetApplicationSettings().RandomGameMaxRerolls;
        var maxRerolls = target.MaxRetries;
        for (var i = 0; i < maxRerolls; i++)
        {
            if (i > 0)
            {
                target.SetBusy(true,
                    "PICKING_A_RANDOM_GAME_FOR_YOU_ATTEMPT_NR_OF_TOT".GetLocalizedString(i + 1, maxRerolls));
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

            var installedGame = await _gameLinkDataService.GetGameByLinks(LinkType.Download,
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

                        vm.Game.GameMetadata.InstallDirectory = mapped.InstalledGame!.GameMetadata.InstallDirectory;
                        vm.Game.GameMetadata.ExecutablePath = mapped.InstalledGame!.GameMetadata.ExecutablePath;
                        vm.Game.GameMetadata.Id = mapped.InstalledGame!.GameMetadata.Id;
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