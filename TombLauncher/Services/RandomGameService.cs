using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using CommunityToolkit.Mvvm.DependencyInjection;
using TombLauncher.Contracts.Downloaders;
using TombLauncher.Contracts.Enums;
using TombLauncher.Core.Extensions;
using TombLauncher.Data.Database.UnitOfWork;
using TombLauncher.Installers.Downloaders;
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
    public async Task PickRandomGame()
    {
        for (var i = 0; i < 10; i++)
        {
            var downloaders = _settingsService.GetActiveDownloaders();
            var downloaderToUse = downloaders.PickOneAtRandom();

            var games = await downloaderToUse.GetGames(new DownloaderSearchPayload(), CancellationToken.None);

            var random = new Random();
            var pageToFetch = random.Next(downloaderToUse.TotalPages.GetValueOrDefault() - 1);


            var gamePage = await downloaderToUse.FetchPage(pageToFetch, CancellationToken.None);
            var pickedGame = gamePage.PickOneAtRandom();

            var allGamesResult = await _gameDownloadManager.GetGames(new DownloaderSearchPayload()
                { LevelName = pickedGame.Title })
                ;

            var details = await _gameDownloadManager.FetchDetails(pickedGame);

            var installedGame = _gamesUnitOfWork.GetGameByLinks(LinkType.Download,
                allGamesResult.FirstOrDefault().Sources.Select(s => s.DownloadLink).ToList());
            if (installedGame == null)
            {
                var vm = new GameDetailsViewModel(Ioc.Default.GetRequiredService<GameDetailsService>(),
                    new GameWithStatsViewModel(Ioc.Default.GetRequiredService<GameWithStatsService>()) { GameMetadata = _mapper.Map<GameMetadataViewModel>(details) }, _settingsService);
                _navigationManager.StartNavigation(vm);
                return;
            }
        }
    }
}