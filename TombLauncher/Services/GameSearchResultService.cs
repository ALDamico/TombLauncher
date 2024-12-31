﻿using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using JamSoft.AvaloniaUI.Dialogs;
using JamSoft.AvaloniaUI.Dialogs.MsgBox;
using Material.Icons;
using TombLauncher.Contracts.Dtos;
using TombLauncher.Contracts.Enums;
using TombLauncher.Contracts.Localization;
using TombLauncher.Contracts.Progress;
using TombLauncher.Data.Database.UnitOfWork;
using TombLauncher.Installers;
using TombLauncher.Installers.Downloaders;
using TombLauncher.Navigation;
using TombLauncher.ViewModels;

namespace TombLauncher.Services;

public class GameSearchResultService : IViewService
{
    public GameSearchResultService(GameDownloadManager downloadManager, GamesUnitOfWork gamesUnitOfWork, TombRaiderLevelInstaller levelInstaller,
        TombRaiderEngineDetector engineDetector, ILocalizationManager localizationManager, NavigationManager navigationManager,
        IMessageBoxService messageBoxService, IDialogService dialogService, MapperConfiguration mapperConfiguration, NotificationService notificationService, GameWithStatsService gameWithStatsService)
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
        _mapper = mapperConfiguration.CreateMapper();
        _notificationService = notificationService;
        _gameWithStatsService = gameWithStatsService;
    }

    private NotificationService _notificationService;
    private readonly GameWithStatsService _gameWithStatsService;
    private CancellationTokenSource _cancellationTokenSource;
    public GameDownloadManager GameDownloadManager { get; }
    public GamesUnitOfWork GamesUnitOfWork { get; }
    public TombRaiderLevelInstaller LevelInstaller { get; }
    public TombRaiderEngineDetector EngineDetector { get; }
    public ILocalizationManager LocalizationManager { get; }
    public NavigationManager NavigationManager { get; }
    public IMessageBoxService MessageBoxService { get; }
    public IDialogService DialogService { get; }
    private IMapper _mapper;
    
    public bool CanInstall(MultiSourceGameSearchResultMetadataViewModel obj)
    {
        if (obj == null) return false;
        var links = obj.Sources.Select(s => s.DownloadLink).ToList();
        var gameDto = GamesUnitOfWork.GetGameByLinks(LinkType.Download, links);
        return gameDto == null;
    }

    public async Task Install(MultiSourceGameSearchResultMetadataViewModel gameToInstall)
    {
        var installProgress = new InstallProgressViewModel();
        var notificationViewModel = new NotificationViewModel()
        {
            Title = gameToInstall.Title,
            Content = installProgress,
            OpenIcon = MaterialIconKind.Play,
            CancelCommand = new AsyncRelayCommand(async () =>
            {
                await CancelInstall();
                installProgress.Message = $"Download cancelled";
                installProgress.IsDownloading = false;
                installProgress.IsInstalling = false;
                _cancellationTokenSource = new CancellationTokenSource();
            }),
            IsCancelable = true,
            IsOpenable = true,
            OpenCommand = new AsyncRelayCommand<GameMetadataDto>((dto) =>
                {
                    if (dto == null)
                        return Task.CompletedTask;
                    return _gameWithStatsService.PlayGame(dto.Id);
                },
                (dto) => dto?.Id != default)
        };
        await _notificationService.AddNotification(notificationViewModel);
        gameToInstall.InstallProgress = installProgress;
        var gameToInstallDto = _mapper.Map<MultiSourceSearchResultMetadataDto>(gameToInstall);
        gameToInstall.IsInstalling = true;
        
        var downloadPath = await GameDownloadManager.DownloadGame(gameToInstallDto, new Progress<DownloadProgressInfo>(
            p =>
            {
                installProgress.IsDownloading = true;
                installProgress.IsInstalling = false;
                installProgress.Message = "Downloading...";
                installProgress.TotalBytes = p.TotalBytes;
                installProgress.CurrentBytes = p.BytesDownloaded;
                installProgress.DownloadSpeed = p.DownloadSpeed;
            }), _cancellationTokenSource.Token);
        Dispatcher.UIThread.Invoke(() =>
        {
            gameToInstall.InstallProgress.TotalBytes = 0;
            gameToInstall.InstallProgress.DownloadSpeed = 0;
        });
        var hashCalculator = Ioc.Default.GetRequiredService<GameFileHashCalculator>();
        var hashes = await hashCalculator.CalculateHashes(downloadPath);
        if (GamesUnitOfWork.ExistsHashes(hashes, out var foundGameId))
        {
            var gameId = foundGameId.GetValueOrDefault();
            var result = await MessageBoxService.Show("Game already installed",
                "This game is already installed. Do you want to install anyway?", MsgBoxButton.YesNo,
                MsgBoxImage.Question);
            if (result.ButtonResult == MsgBoxButtonResult.No)
            {
                GamesUnitOfWork.SaveLink(new GameLinkDto()
                    { Link = gameToInstall.DownloadLink, LinkType = LinkType.Download, GameId = gameId, BaseUrl = gameToInstall.BaseUrl, DisplayName = gameToInstall.SourceSiteDisplayName});
                GamesUnitOfWork.SaveLink(new GameLinkDto()
                    { Link = gameToInstall.DetailsLink, LinkType = LinkType.Details, GameId = gameId, BaseUrl = gameToInstall.BaseUrl, DisplayName = gameToInstall.SourceSiteDisplayName });
                if (gameToInstall.HasReviews)
                {
                    GamesUnitOfWork.SaveLink(new GameLinkDto()
                        { Link = gameToInstall.ReviewsLink, LinkType = LinkType.Reviews, GameId = gameId, BaseUrl = gameToInstall.BaseUrl, DisplayName = gameToInstall.SourceSiteDisplayName });
                }

                if (gameToInstall.HasWalkthrough)
                {
                    GamesUnitOfWork.SaveLink(new GameLinkDto()
                        { Link = gameToInstall.WalkthroughLink, LinkType = LinkType.Walkthrough, GameId = gameId, BaseUrl = gameToInstall.BaseUrl, DisplayName = gameToInstall.SourceSiteDisplayName });
                }
                return;
            }
        }

        var allDetails = await GameDownloadManager.FetchAllDetails(gameToInstallDto);

        //var engine = _engineDetector.Detect(downloadPath);
        var dto = await GameDownloadManager.FetchDetails(gameToInstallDto);
        dto.Guid = Guid.NewGuid();
        notificationViewModel.OpenCmdParam = dto;
        var installLocation = await LevelInstaller.Install(downloadPath, dto, new Progress<CopyProgressInfo>(a =>
        {
            installProgress.IsDownloading = false;
            installProgress.IsInstalling = true;
            installProgress.Message = "Installing...";
            installProgress.InstallPercentage = a.Percentage.GetValueOrDefault();
            installProgress.CurrentFileName = a.CurrentFileName;
        }));
        dto.InstallDate = DateTime.Now;
        dto.InstallDirectory = installLocation;
        var detectionResult = EngineDetector.Detect(installLocation);
        dto.ExecutablePath = detectionResult.ExecutablePath;
        dto.UniversalLauncherPath = detectionResult.UniversalLauncherPath;
        dto.GameEngine = detectionResult.GameEngine;
        GamesUnitOfWork.UpsertGame(dto);
        hashes.ForEach(h => h.GameId = dto.Id);
        GamesUnitOfWork.SaveHashes(hashes);
        foreach (var detail in allDetails.Sources)
        {
            GamesUnitOfWork.SaveLink(new GameLinkDto()
            {
                Link = detail.DownloadLink, LinkType = LinkType.Download, GameId = dto.Id,
                BaseUrl = detail.BaseUrl, DisplayName = detail.SourceSiteDisplayName
            });
            GamesUnitOfWork.SaveLink(new GameLinkDto()
            {
                Link = detail.DetailsLink, LinkType = LinkType.Details, GameId = dto.Id,
                BaseUrl = detail.BaseUrl, DisplayName = detail.SourceSiteDisplayName
            });
            if (detail.HasReviews)
            {
                GamesUnitOfWork.SaveLink(new GameLinkDto()
                {
                    Link = detail.ReviewsLink, LinkType = LinkType.Reviews, GameId = dto.Id,
                    BaseUrl = detail.BaseUrl, DisplayName = detail.SourceSiteDisplayName
                });
            }

            if (detail.HasWalkthrough)
            {
                GamesUnitOfWork.SaveLink(new GameLinkDto()
                {
                    Link = detail.WalkthroughLink, LinkType = LinkType.Walkthrough, GameId = dto.Id,
                    BaseUrl = detail.BaseUrl, DisplayName = detail.SourceSiteDisplayName
                });
            }
        }

        installProgress.IsInstalling = false;
        installProgress.IsDownloading = false;
        installProgress.Message = "Install complete";
        notificationViewModel.IsCancelable = false;

        GamesUnitOfWork.Save();
    }

    public async Task CancelInstall()
    {
        await _cancellationTokenSource.CancelAsync();
        _cancellationTokenSource = new CancellationTokenSource();
    }

    public bool CanCancelInstall(MultiSourceGameSearchResultMetadataViewModel target) => target.IsInstalling;
}