using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.DependencyInjection;
using JamSoft.AvaloniaUI.Dialogs;
using JamSoft.AvaloniaUI.Dialogs.MsgBox;
using TombLauncher.Contracts.Localization;
using TombLauncher.Data.Database.UnitOfWork;
using TombLauncher.Data.Dto;
using TombLauncher.Data.Models;
using TombLauncher.Installers;
using TombLauncher.Installers.Downloaders;
using TombLauncher.Localization;
using TombLauncher.Navigation;
using TombLauncher.Progress;
using TombLauncher.ViewModels;

namespace TombLauncher.Services;

public class GameSearchResultService : IViewService
{
    public GameSearchResultService(GameDownloadManager downloadManager, GamesUnitOfWork gamesUnitOfWork, TombRaiderLevelInstaller levelInstaller,
        TombRaiderEngineDetector engineDetector, ILocalizationManager localizationManager, NavigationManager navigationManager,
        IMessageBoxService messageBoxService, IDialogService dialogService)
    {
        GameDownloadManager = downloadManager;
        GamesUnitOfWork = gamesUnitOfWork;
        LevelInstaller = levelInstaller;
        EngineDetector = engineDetector;
        LocalizationManager = localizationManager;
        NavigationManager = navigationManager;
        MessageBoxService = messageBoxService;
        DialogService = dialogService;
        _cancellationTokenSource = new CancellationTokenSource();
    }
    private CancellationTokenSource _cancellationTokenSource;
    public GameDownloadManager GameDownloadManager { get; }
    public GamesUnitOfWork GamesUnitOfWork { get; }
    public TombRaiderLevelInstaller LevelInstaller { get; }
    public TombRaiderEngineDetector EngineDetector { get; }
    public ILocalizationManager LocalizationManager { get; }
    public NavigationManager NavigationManager { get; }
    public IMessageBoxService MessageBoxService { get; }
    public IDialogService DialogService { get; }
    
    public bool CanInstall(MultiSourceGameSearchResultMetadataViewModel obj)
    {
        if (obj == null) return false;
        var links = obj.Sources.Select(s => s.DownloadLink).ToList();
        var gameDto = GamesUnitOfWork.GetGameByLinks(LinkType.Download, links);
        return gameDto == null;
    }

    public async Task Install(MultiSourceGameSearchResultMetadataViewModel gameToInstall)
    {
        gameToInstall.IsInstalling = true;
        var downloadPath = await GameDownloadManager.DownloadGame(gameToInstall, new Progress<DownloadProgressInfo>(
            p =>
            {
                gameToInstall.TotalBytes = p.TotalBytes;
                gameToInstall.CurrentBytes = p.BytesDownloaded;
                gameToInstall.DownloadSpeed = p.DownloadSpeed;
            }), _cancellationTokenSource.Token);
        Dispatcher.UIThread.Invoke(() =>
        {
            gameToInstall.TotalBytes = 0;
            gameToInstall.DownloadSpeed = 0;
        });
        var hashCalculator = Ioc.Default.GetRequiredService<GameFileHashCalculator>();
        var hashes = await hashCalculator.CalculateHashes(downloadPath);
        if (GamesUnitOfWork.ExistsHashes(hashes))
        {
            var result = await MessageBoxService.Show("Game already installed",
                "This game is already installed. Do you want to install anyway?", MsgBoxButton.YesNo,
                MsgBoxImage.Question);
            if (result.ButtonResult == MsgBoxButtonResult.No)
            {
                return;
            }
        }

        //var engine = _engineDetector.Detect(downloadPath);
        var dto = await GameDownloadManager.FetchDetails(gameToInstall);
        dto.Guid = Guid.NewGuid();
        var installLocation = await LevelInstaller.Install(downloadPath, dto, new Progress<CopyProgressInfo>(a =>
        {
            gameToInstall.TotalBytes = 100;
            gameToInstall.CurrentBytes = a.Percentage.GetValueOrDefault();
        }));
        dto.InstallDate = DateTime.Now;
        dto.InstallDirectory = installLocation;
        var exePath = EngineDetector.GetGameExecutablePath(installLocation);
        dto.ExecutablePath = exePath;
        GamesUnitOfWork.UpsertGame(dto);
        hashes.ForEach(h => h.GameId = dto.Id);
        GamesUnitOfWork.SaveHashes(hashes);
        GamesUnitOfWork.SaveLink(new GameLinkDto()
            { Link = gameToInstall.DownloadLink, LinkType = LinkType.Download, GameId = dto.Id });
        GamesUnitOfWork.SaveLink(new GameLinkDto()
            { Link = gameToInstall.DetailsLink, LinkType = LinkType.Details, GameId = dto.Id });
        if (gameToInstall.HasReviews)
        {
            GamesUnitOfWork.SaveLink(new GameLinkDto()
                { Link = gameToInstall.ReviewsLink, LinkType = LinkType.Reviews, GameId = dto.Id });
        }

        if (gameToInstall.HasWalkthrough)
        {
            GamesUnitOfWork.SaveLink(new GameLinkDto()
                { Link = gameToInstall.WalkthroughLink, LinkType = LinkType.Walkthrough, GameId = dto.Id });
        }

        GamesUnitOfWork.Save();
    }

    public async Task CancelInstall()
    {
        await _cancellationTokenSource.CancelAsync();
        _cancellationTokenSource = new CancellationTokenSource();
    }

    public bool CanCancelInstall(MultiSourceGameSearchResultMetadataViewModel target) => target.IsInstalling;
}