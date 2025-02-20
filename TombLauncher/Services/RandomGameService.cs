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
using TombLauncher.Data.Database.UnitOfWork;
using TombLauncher.Installers.Downloaders;
using TombLauncher.Localization.Extensions;
using TombLauncher.Navigation;
using TombLauncher.ViewModels;
using TombLauncher.ViewModels.Pages;

namespace TombLauncher.Services;

public class RandomGameService
{
    public RandomGameService()
    {
        _settingsService = Ioc.Default.GetRequiredService<SettingsService>();
        _gamesUnitOfWork = Ioc.Default.GetRequiredService<GamesUnitOfWork>();
        _gameDownloadManager = Ioc.Default.GetRequiredService<GameDownloadManager>();
        _navigationManager = Ioc.Default.GetRequiredService<NavigationManager>();
        _mapper = Ioc.Default.GetRequiredService<MapperConfiguration>().CreateMapper();
    }

    private SettingsService _settingsService;
    private GamesUnitOfWork _gamesUnitOfWork;
    private GameDownloadManager _gameDownloadManager;
    private NavigationManager _navigationManager;
    private IMapper _mapper;

    public async Task PickRandomGame(RandomGameViewModel target)
    {
        var currentPage = _navigationManager.GetCurrentPage();
        var maxRerolls = target.MaxRetries;
        for (var i = 0; i < maxRerolls; i++)
        {
            if (i > 0)
            {
                currentPage.SetBusy(true,
                    "Picking a random game for you... (Attempt NR of TOT)".GetLocalizedString(i + 1, maxRerolls));
            }

            var downloaders = _settingsService.GetActiveDownloaders();
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
                var searchResultService = Ioc.Default.GetRequiredService<GameSearchResultService>();
                var mapped = _mapper.Map<MultiSourceGameSearchResultMetadataViewModel>(candidate);
                var vm = new GameDetailsViewModel(
                    new GameWithStatsViewModel()
                        { GameMetadata = _mapper.Map<GameMetadataViewModel>(details) });
                vm.InstallCmd = new AsyncRelayCommand(async () =>
                    {
                        vm.SetBusy(true, $"Installing {mapped.Title}");
                        await searchResultService.Install(
                            mapped);

                        vm.Game.GameMetadata.InstallDirectory = mapped.InstalledGame.GameMetadata.InstallDirectory;
                        vm.Game.GameMetadata.ExecutablePath = mapped.InstalledGame.GameMetadata.ExecutablePath;
                        vm.Game.GameMetadata.Id = mapped.InstalledGame.GameMetadata.Id;
                        vm.Game.RaiseCanExecuteChanged(vm.Game.PlayCmd);
                        vm.Game.RaiseCanExecuteChanged(vm.Game.LaunchSetupCmd);
                        vm.Game.RaiseCanExecuteChanged(vm.Game.OpenCmd);
                        vm.InstallCmd = null;
                        vm.SetBusy(false);
                    }, () => mapped.InstalledGame == null)
                    ;
                await _navigationManager.StartNavigationAsync(Task.FromResult<PageViewModel>(vm));
                return;
            }
        }

        target.AttemptsExpired = true;
    }
}