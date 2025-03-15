using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using JamSoft.AvaloniaUI.Dialogs;
using JamSoft.AvaloniaUI.Dialogs.MsgBox;
using Material.Icons;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Microsoft.Extensions.Logging;
using TombLauncher.Contracts.Downloaders;
using TombLauncher.Contracts.Enums;
using TombLauncher.Contracts.Localization;
using TombLauncher.Contracts.Progress;
using TombLauncher.Core.Dtos;
using TombLauncher.Core.Extensions;
using TombLauncher.Data.Database.UnitOfWork;
using TombLauncher.Extensions;
using TombLauncher.Installers;
using TombLauncher.Installers.Downloaders;
using TombLauncher.Navigation;
using TombLauncher.ViewModels;

namespace TombLauncher.Services;

public class GameSearchResultService : IViewService
{
    public GameSearchResultService(GameDownloadManager downloadManager, GamesUnitOfWork gamesUnitOfWork,
        TombRaiderLevelInstaller levelInstaller,
        TombRaiderEngineDetector engineDetector, ILocalizationManager localizationManager,
        NavigationManager navigationManager,
        IMessageBoxService messageBoxService, IDialogService dialogService, MapperConfiguration mapperConfiguration,
        NotificationService notificationService, GameWithStatsService gameWithStatsService)
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
        _logger = Ioc.Default.GetRequiredService<ILogger<GameSearchResultService>>();
    }

    private ILogger<GameSearchResultService> _logger;

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
    private string _downloadPath;
    private string _installPath;
    private int? _installedGameId;
    private InstallProgressViewModel _installProgress;
    private NotificationViewModel _notificationViewModel;

    public bool CanInstall(MultiSourceGameSearchResultMetadataViewModel obj)
    {
        if (obj == null) return false;
        var links = obj.Sources.Select(s => s.DownloadLink).ToList();
        var gameDto = GamesUnitOfWork.GetGameByLinks(LinkType.Download, links).GetAwaiter().GetResult();
        return gameDto is not { IsInstalled: true };
    }

    public async Task Install(MultiSourceGameSearchResultMetadataViewModel gameToInstall)
    {
        _logger.LogInformation("Attempting install for {GameTitle}", gameToInstall.Title);
        _installProgress = new InstallProgressViewModel();
        await InitNotificationViewModel(gameToInstall);
        
        gameToInstall.InstallProgress = _installProgress;
        var gameToInstallDto = _mapper.Map<MultiSourceSearchResultMetadataDto>(gameToInstall);

        _logger.LogInformation("Started downloading {GameTitle} from {DownloadUrl}", gameToInstall.Title,
            gameToInstall.DownloadLink);
        foreach (var source in gameToInstallDto.Sources)
        {
            try
            {
                _downloadPath = await GameDownloadManager.DownloadGame(source,
                    new Progress<DownloadProgressInfo>(
                        p =>
                        {
                            _installProgress.IsDownloading = true;
                            _installProgress.IsInstalling = false;
                            _installProgress.Message = "Downloading...";
                            _installProgress.TotalBytes = p.TotalBytes;
                            _installProgress.CurrentBytes = p.BytesDownloaded;
                            _installProgress.DownloadSpeed = p.DownloadSpeed;
                        }), _cancellationTokenSource.Token);
                Dispatcher.UIThread.Invoke(() =>
                {
                    gameToInstall.InstallProgress.TotalBytes = 0;
                    gameToInstall.InstallProgress.DownloadSpeed = 0;
                });
                break;
            }
            catch (HttpRequestException httpEx)
            {
                if (httpEx.HttpRequestError == HttpRequestError.SecureConnectionError &&
                    httpEx.InnerException?.Message.Contains("RemoteCertificateNameMismatch",
                        StringComparison.InvariantCultureIgnoreCase) == true)
                {
                    _logger.LogError(httpEx,
                        "Error downloading game. This is potentially due to the download link redirecting to an external website.");
                }
            }
        }

        if (!await EnsureDownloadPathNotNull(gameToInstall)) 
            return;

        _logger.LogInformation("Calculating hashes for {GameTitle}", gameToInstall.Title);
        var hashCalculator = Ioc.Default.GetRequiredService<GameFileHashCalculator>();
        var hashes = await hashCalculator.CalculateHashes(_downloadPath);
        if (await CheckGameAlreadyInstalled(gameToInstall, hashes)) 
            return;

        var allDetails = await GameDownloadManager.FetchAllDetails(gameToInstallDto);
        var dto = await GameDownloadManager.FetchDetails(gameToInstallDto);
        _installedGameId = dto.Id;
        if (dto.InstallDirectory.IsNotNullOrWhiteSpace())
        {
            _installPath = dto.InstallDirectory;
        }

        dto.Guid = Guid.NewGuid();
        if (gameToInstall.InstalledGame != null)
        {
            dto.Id = gameToInstall.InstalledGame.GameMetadata.Id;
            dto.Guid = gameToInstall.InstalledGame.GameMetadata.Guid;
            dto.InstallDirectory = gameToInstall.InstalledGame.GameMetadata.InstallDirectory;
        }

        _notificationViewModel.OpenCmdParam = dto;
        _logger.LogInformation("Starting install for {GameTitle}", gameToInstall.Title);
        try
        {
            var installLocation = await LevelInstaller.Install(_downloadPath, dto, _cancellationTokenSource.Token,
                new Progress<CopyProgressInfo>(a =>
                {
                    _installProgress.IsDownloading = false;
                    _installProgress.IsInstalling = true;
                    _installProgress.Message = "Installing...";
                    _installProgress.InstallPercentage = a.Percentage.GetValueOrDefault();
                    _installProgress.CurrentFileName = a.CurrentFileName;
                }));

            dto.InstallDate = DateTime.Now;
            dto.IsInstalled = true;
            dto.InstallDirectory = installLocation;
            dto.Difficulty = gameToInstall.Difficulty;
            dto.Length = gameToInstall.Length;
            DetectGameEngine(installLocation, dto);
            _logger.LogInformation("Saving data for {GameTitle} to database", gameToInstall.Title);
            await GamesUnitOfWork.UpsertGame(dto);
            _installedGameId = dto.Id;
            if (_installPath.IsNotNullOrWhiteSpace())
            {
                _installPath = dto.InstallDirectory;
            }

            hashes.ForEach(h => h.GameId = dto.Id);
            await GamesUnitOfWork.SaveHashes(hashes);
            await SaveLinks(allDetails, dto);
        }
        catch (TaskCanceledException)
        {
            await AfterInstallCleanup();
            _installProgress.Message = $"Install cancelled";
            _installProgress.IsDownloading = false;
            _installProgress.IsInstalling = false;
            _installProgress.ProcessStarted = false;
            return;
        }

        _installProgress.IsInstalling = false;
        _installProgress.IsDownloading = false;
        _installProgress.ProcessStarted = false;
        _installProgress.InstallCompleted = true;
        _installProgress.Message = "Install complete";
        if (_notificationViewModel != null)
        {
            _notificationViewModel.IsCancelable = false;
            _notificationViewModel.IsDismissable = true;
        }
        
        await GamesUnitOfWork.Save();
        await AfterInstallCleanup();
        gameToInstall.InstalledGame =
            _mapper.Map<GameWithStatsViewModel>(await GamesUnitOfWork.GetGameWithStats(dto.Id));
        _notificationViewModel = null;
        _installedGameId = null;
        _downloadPath = null;
        _logger.LogInformation("Installation for {GameTitle} complete", gameToInstall.Title);
    }

    private async Task<bool> CheckGameAlreadyInstalled(MultiSourceGameSearchResultMetadataViewModel gameToInstall, List<GameHashDto> hashes)
    {
        if (GamesUnitOfWork.ExistsHashes(hashes, out var foundGameId))
        {
            _logger.LogWarning("Game {GameTitle} already installed", gameToInstall.Title);
            var gameId = foundGameId.GetValueOrDefault();
            var result = await MessageBoxService.ShowLocalized("The same mod is already installed",
                "The same mod is already installed TEXT",
                MsgBoxButton.YesNo,
                MsgBoxImage.Error,
                noButtonText: "Cancel",
                yesButtonText: "Install anyway");
            if (result.ButtonResult == MsgBoxButtonResult.No)
            {
                _logger.LogInformation("Won't install already existing game {GameTitle}", gameToInstall.Title);
                await GamesUnitOfWork.SaveLink(new GameLinkDto()
                {
                    Link = gameToInstall.DownloadLink, LinkType = LinkType.Download, GameId = gameId,
                    BaseUrl = gameToInstall.BaseUrl, DisplayName = gameToInstall.SourceSiteDisplayName
                });
                await GamesUnitOfWork.SaveLink(new GameLinkDto()
                {
                    Link = gameToInstall.DetailsLink, LinkType = LinkType.Details, GameId = gameId,
                    BaseUrl = gameToInstall.BaseUrl, DisplayName = gameToInstall.SourceSiteDisplayName
                });
                if (gameToInstall.HasReviews)
                {
                    await GamesUnitOfWork.SaveLink(new GameLinkDto()
                    {
                        Link = gameToInstall.ReviewsLink, LinkType = LinkType.Reviews, GameId = gameId,
                        BaseUrl = gameToInstall.BaseUrl, DisplayName = gameToInstall.SourceSiteDisplayName
                    });
                }

                if (gameToInstall.HasWalkthrough)
                {
                    await GamesUnitOfWork.SaveLink(new GameLinkDto()
                    {
                        Link = gameToInstall.WalkthroughLink, LinkType = LinkType.Walkthrough, GameId = gameId,
                        BaseUrl = gameToInstall.BaseUrl, DisplayName = gameToInstall.SourceSiteDisplayName
                    });
                }

                return true;
            }
            else
            {
                _logger.LogWarning("Will install anyway");
            }
        }

        return false;
    }

    private async Task<bool> EnsureDownloadPathNotNull(MultiSourceGameSearchResultMetadataViewModel gameToInstall)
    {
        if (_downloadPath == null)
        {
            _logger.LogError("Download failed from all sources.");
            _notificationService.RemoveNotification(_notificationViewModel);
            await _notificationService.AddErrorNotificationAsync(gameToInstall.Title,
                "Download failed from all sources. This is likely due to a download link redirecting to an external website.",
                MaterialIconKind.Warning);
            return false;
        }

        return true;
    }

    private async Task InitNotificationViewModel(MultiSourceGameSearchResultMetadataViewModel gameToInstall)
    {
        _notificationViewModel = new NotificationViewModel()
        {
            Title = gameToInstall.Title,
            Content = _installProgress,
            OpenIcon = MaterialIconKind.Play,
            CancelCommand = new AsyncRelayCommand(async () =>
            {
                await CancelInstall();
                _installProgress.Message = $"Download cancelled";
                _installProgress.IsDownloading = false;
                _installProgress.IsInstalling = false;
                _installProgress.ProcessStarted = false;
                if (_notificationViewModel != null)
                {
                    _notificationViewModel.IsOpenable = false;
                    _notificationViewModel.IsDismissable = true;
                    _notificationViewModel.IsCancelable = false;
                }
            }),
            IsCancelable = true,
            IsOpenable = true,
            OpenCommand = new AsyncRelayCommand<GameMetadataDto>((dto) =>
                {
                    if (dto == null)
                        return Task.CompletedTask;
                    return _gameWithStatsService.PlayGame(dto.Id);
                },
                (dto) => dto?.Id != null && _installProgress.InstallCompleted)
        };
        await _notificationService.AddNotificationAsync(_notificationViewModel);
    }

    private void DetectGameEngine(string installLocation, IGameMetadata dto)
    {
        var detectionResult = EngineDetector.Detect(installLocation);
        dto.ExecutablePath = detectionResult.ExecutablePath;
        dto.GameEngine = detectionResult.GameEngine;
        dto.SetupExecutable = detectionResult.SetupExecutablePath;
        dto.SetupExecutableArgs = detectionResult.SetupArgs;
        dto.CommunitySetupExecutable = detectionResult.CommunitySetupExecutablePath;
    }

    private async Task SaveLinks(IMultiSourceSearchResultMetadata allDetails, IGameMetadata dto)
    {
        foreach (var detail in allDetails.Sources)
        {
            await GamesUnitOfWork.SaveLink(new GameLinkDto()
            {
                Link = detail.DownloadLink, LinkType = LinkType.Download, GameId = dto.Id,
                BaseUrl = detail.BaseUrl, DisplayName = detail.SourceSiteDisplayName
            });
            await GamesUnitOfWork.SaveLink(new GameLinkDto()
            {
                Link = detail.DetailsLink, LinkType = LinkType.Details, GameId = dto.Id,
                BaseUrl = detail.BaseUrl, DisplayName = detail.SourceSiteDisplayName
            });
            if (detail.HasReviews)
            {
                await GamesUnitOfWork.SaveLink(new GameLinkDto()
                {
                    Link = detail.ReviewsLink, LinkType = LinkType.Reviews, GameId = dto.Id,
                    BaseUrl = detail.BaseUrl, DisplayName = detail.SourceSiteDisplayName
                });
            }

            if (detail.HasWalkthrough)
            {
                await GamesUnitOfWork.SaveLink(new GameLinkDto()
                {
                    Link = detail.WalkthroughLink, LinkType = LinkType.Walkthrough, GameId = dto.Id,
                    BaseUrl = detail.BaseUrl, DisplayName = detail.SourceSiteDisplayName
                });
            }
        }
    }

    public async Task CancelInstall()
    {
        _logger.LogInformation("Install cancellation requested");
        await _cancellationTokenSource.CancelAsync();
        _installProgress.Message = $"Download cancelled";
        _installProgress.IsDownloading = false;
        _installProgress.IsInstalling = false;
        _installProgress.ProcessStarted = false;
        _cancellationTokenSource = new CancellationTokenSource();
        if (_notificationViewModel != null)
        {
            _notificationViewModel.IsOpenable = false;
        }

        await AfterInstallCleanup();
        _logger.LogInformation("Installation canceled");
    }

    private async Task AfterInstallCleanup()
    {
        if (_downloadPath == null)
            return;
        try
        {
            if (Directory.Exists(_downloadPath))
                Directory.Delete(_downloadPath);

            // _downloadPath may be a file
            if (File.Exists(_downloadPath))
                File.Delete(_downloadPath);
        }
        catch (IOException)
        {
        }

        if (_installedGameId != null && _installPath.IsNotNullOrWhiteSpace())
        {
            await _gameWithStatsService.Uninstall(_installPath, _installedGameId.GetValueOrDefault());
        }

        _downloadPath = null;
        _notificationViewModel.IsDismissable = true;
        _notificationViewModel.IsCancelable = false;
        _notificationViewModel = null;
        _installedGameId = null;
    }
}