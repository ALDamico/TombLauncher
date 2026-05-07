using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.Input;
using JamSoft.AvaloniaUI.Dialogs.MsgBox;
using IconPacks.Avalonia.RemixIcon;
using Microsoft.Extensions.Logging;
using TombLauncher.Contracts.Downloaders;
using TombLauncher.Contracts.EngineDetectors;
using TombLauncher.Contracts.Enums;
using TombLauncher.Contracts.Localization;
using TombLauncher.Contracts.Progress;
using TombLauncher.Core.Dtos;
using TombLauncher.Core.Extensions;
using TombLauncher.Data.Database.Services;
using TombLauncher.Extensions;
using TombLauncher.Installers;
using TombLauncher.Installers.Downloaders;
using TombLauncher.Mappers;
using TombLauncher.ViewModels;

namespace TombLauncher.Services;

public class GameSearchResultService : IViewService
{
    public GameSearchResultService(ViewServiceContext viewContext, GameDataService gameDataService,
        GameLinkDataService gameLinkDataService, GameHashDataService gameHashDataService,
        TombRaiderLevelInstaller levelInstaller,
        IEngineDetector engineDetector,
        GameDownloadManager downloadManager,
        NotificationService notificationService, GameWithStatsService gameWithStatsService,
        ILogger<GameSearchResultService> logger, GameFileHashCalculator hashCalculator,
        IAppFileOperationsService appFileOperations, ISettingsProvider settingsProvider,
        GameMetadataMapper gameMetadataMapper,
        SearchMapper searchMapper)
    {
        ViewContext = viewContext;
        _gameDownloadManager = downloadManager;
        _gameDataService = gameDataService;
        _gameLinkDataService = gameLinkDataService;
        _gameHashDataService = gameHashDataService;
        _levelInstaller = levelInstaller;
        _engineDetector = engineDetector;
        _cancellationTokenSource = new CancellationTokenSource();
        _notificationService = notificationService;
        _gameWithStatsService = gameWithStatsService;
        _logger = logger;
        _hashCalculator = hashCalculator;
        _appFileOperations = appFileOperations;
        _settingsProvider = settingsProvider;
        _gameMetadataMapper = gameMetadataMapper;
        _searchMapper = searchMapper;
    }

    public ViewServiceContext ViewContext { get; }
    private readonly ILogger<GameSearchResultService> _logger;

    private readonly NotificationService _notificationService;
    private readonly GameWithStatsService _gameWithStatsService;
    private CancellationTokenSource _cancellationTokenSource;
    private readonly GameDownloadManager _gameDownloadManager;
    private readonly GameDataService _gameDataService;
    private readonly GameLinkDataService _gameLinkDataService;
    private readonly GameHashDataService _gameHashDataService;
    private readonly TombRaiderLevelInstaller _levelInstaller;
    private readonly IEngineDetector _engineDetector;
    public ILocalizationManager LocalizationManager => ViewContext.LocalizationManager;
    public NavigationManager NavigationManager => ViewContext.NavigationManager;
    private readonly GameFileHashCalculator _hashCalculator;
    private readonly IAppFileOperationsService _appFileOperations;
    private readonly ISettingsProvider _settingsProvider;
    private readonly GameMetadataMapper _gameMetadataMapper;
    private readonly SearchMapper _searchMapper;
    private string? _downloadPath;
    private string? _installPath;
    private int? _installedGameId;
    private InstallProgressViewModel? _installProgress;
    private NotificationViewModel? _notificationViewModel;
    private IGameSearchResultMetadata? _preferredSource;

    public async Task<bool> CanInstall(MultiSourceGameSearchResultMetadataViewModel obj)
    {
        if (obj.DownloadLink.IsNullOrWhiteSpace())
            return false;
        var links = obj.Sources
            .Select(s => s.DownloadLink).Where(downloadLink => downloadLink.IsNotNullOrWhiteSpace())
            .Cast<string>()
            .ToList();
        if (links.Count == 0)
        {
            return false;
        }

        var gameDto = await _gameLinkDataService.GetGameByLinks(LinkType.Download, links);
        return gameDto is not { IsInstalled: true };
    }

    public async Task Install(MultiSourceGameSearchResultMetadataViewModel gameToInstall)
    {
        _logger.LogInformation("Attempting install for {GameTitle}", gameToInstall.Title);
        _installProgress = new InstallProgressViewModel();
        await InitNotificationViewModel(gameToInstall);

        gameToInstall.InstallProgress = _installProgress;
        var gameToInstallDto = _searchMapper.ToMergedDto(gameToInstall);

        _logger.LogInformation("Started downloading {GameTitle} from {DownloadUrl}", gameToInstall.Title,
            gameToInstall.DownloadLink);
        IGameSearchResultMetadata? successfulSource = null;
        var sourcesToTry = _preferredSource != null
            ? gameToInstallDto.Sources.Where(s => s.DownloadLink == _preferredSource.DownloadLink)
            : gameToInstallDto.Sources;
        foreach (var source in sourcesToTry)
        {
            try
            {
                _downloadPath = await _gameDownloadManager.DownloadGame(source,
                    new Progress<DownloadProgressInfo>(
                        p =>
                        {
                            _installProgress.InstallStatus = InstallStatus.Downloading;
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
                successfulSource = source;
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

        _installProgress.InstallStatus = InstallStatus.Indeterminate;
        _installProgress.Message = LocalizationManager.GetLocalizedString("VERIFYING_DOWNLOAD");
        var hashes = await _hashCalculator.CalculateHashes(_downloadPath!);
        if (await CheckGameAlreadyInstalled(gameToInstall, hashes))
            return;

        _installProgress.Message = LocalizationManager.GetLocalizedString("FETCHING_GAME_DETAILS");
        var allDetails = await _gameDownloadManager.FetchAllDetails( _settingsProvider.GetActiveDownloaders(), gameToInstallDto);
        var dto = await _gameDownloadManager.FetchDetails(gameToInstallDto);
        if (dto == null)
        {
            _logger.LogWarning("Download Manager failed to fetch details for game {GameTitle}", gameToInstall.Title);
            return;
        }
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

        if (_notificationViewModel != null)
        {
            _notificationViewModel.OpenCmdParam = dto;
        }
        _logger.LogInformation("Starting install for {GameTitle}", gameToInstall.Title);
        try
        {
            var installLocation = await _levelInstaller.Install(_downloadPath!, dto, _cancellationTokenSource.Token,
                new Progress<CopyProgressInfo>(a =>
                {
                    if (_installProgress == null) return;
                    _installProgress.InstallStatus = InstallStatus.Installing;
                    _installProgress.Message = "Installing...";
                    _installProgress.InstallPercentage = a.Percentage.GetValueOrDefault();
                    _installProgress.CurrentFileName = a.CurrentFileName ?? string.Empty;
                }));

            dto.InstallDate = DateTime.Now;
            dto.IsInstalled = true;
            dto.InstallDirectory = installLocation;
            dto.Difficulty = gameToInstall.Difficulty;
            dto.Length = gameToInstall.Length;
            DetectGameEngine(installLocation, dto);
            _logger.LogInformation("Saving data for {GameTitle} to database", gameToInstall.Title);
            await _gameDataService.UpsertGame(dto);
            _installedGameId = dto.Id;
            if (_installPath.IsNotNullOrWhiteSpace())
            {
                _installPath = dto.InstallDirectory;
            }

            hashes.ForEach(h => h.GameId = dto.Id);
            await _gameHashDataService.SaveHashes(hashes);
            var installedFromLinkId = await SaveLinks(allDetails, dto, successfulSource?.DownloadLink);
            if (installedFromLinkId != 0)
                await _gameDataService.SetInstalledFromLink(dto.Id, installedFromLinkId);
        }
        catch (TaskCanceledException)
        {
            await AfterInstallCleanup();
            if (_installProgress != null)
            {
                _installProgress.Message = $"Install cancelled";
                _installProgress.InstallStatus = InstallStatus.Canceled;
            }
            return;
        }

        if (_installProgress != null)
        {
            _installProgress.InstallStatus = InstallStatus.Completed;
            _installProgress.Message = "Install complete";
        }
        if (_notificationViewModel != null)
        {
            _notificationViewModel.IsCancelable = false;
            _notificationViewModel.IsDismissable = true;
        }

        await AfterInstallCleanup();
        gameToInstall.InstalledGame =
            _gameMetadataMapper.ToViewModel(await _gameDataService.GetGameWithStats(dto.Id), _gameWithStatsService);
        _notificationViewModel = null;
        _installedGameId = null;
        _downloadPath = null;
        _logger.LogInformation("Installation for {GameTitle} complete", gameToInstall.Title);
    }

    public async Task InstallFromSource(MultiSourceGameSearchResultMetadataViewModel gameToInstall,
        IGameSearchResultMetadata specificSource)
    {
        _preferredSource = specificSource;
        try
        {
            await Install(gameToInstall);
        }
        finally
        {
            _preferredSource = null;
        }
    }

    private async Task<bool> CheckGameAlreadyInstalled(MultiSourceGameSearchResultMetadataViewModel gameToInstall, List<GameHashDto> hashes)
    {
        var (hashesExists, foundGameId) = await _gameHashDataService.ExistsHashes(hashes, CancellationToken.None);
        if (hashesExists)
        {
            _logger.LogWarning("Game {GameTitle} already installed", gameToInstall.Title);
            var gameId = foundGameId.GetValueOrDefault();
            var result = await ViewContext.PopupService.ShowLocalized("The same mod is already installed",
                "The same mod is already installed TEXT",
                MsgBoxButton.YesNo,
                MsgBoxImage.Error,
                noButtonText: "Cancel",
                yesButtonText: "Install anyway");
            if (result.ButtonResult == MsgBoxButtonResult.No)
            {
                _logger.LogInformation("Won't install already existing game {GameTitle}", gameToInstall.Title);
                var linksToSave = new List<GameLinkDto>()
                {
                    new()
                    {
                        Link = gameToInstall.DownloadLink,
                        LinkType = LinkType.Download,
                        GameId = gameId,
                        BaseUrl = gameToInstall.BaseUrl,
                        DisplayName = gameToInstall.SourceSiteDisplayName
                    },
                    new()
                    {
                        Link = gameToInstall.DetailsLink,
                        LinkType = LinkType.Details,
                        GameId = gameId,
                        BaseUrl = gameToInstall.BaseUrl,
                        DisplayName = gameToInstall.SourceSiteDisplayName
                    }
                };
                if (gameToInstall.HasReviews)
                {
                    linksToSave.Add(new GameLinkDto()
                    {
                        Link = gameToInstall.ReviewsLink,
                        LinkType = LinkType.Reviews,
                        GameId = gameId,
                        BaseUrl = gameToInstall.BaseUrl,
                        DisplayName = gameToInstall.SourceSiteDisplayName
                    });
                }

                if (gameToInstall.HasWalkthrough)
                {
                    linksToSave.Add(new GameLinkDto()
                    {
                        Link = gameToInstall.WalkthroughLink,
                        LinkType = LinkType.Walkthrough,
                        GameId = gameId,
                        BaseUrl = gameToInstall.BaseUrl,
                        DisplayName = gameToInstall.SourceSiteDisplayName
                    });
                }

                await _gameLinkDataService.SaveLinks(linksToSave, _cancellationTokenSource.Token);

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
            if (_notificationViewModel != null)
            {
                _notificationService.RemoveNotification(_notificationViewModel);
            }
            await _notificationService.AddErrorNotificationAsync(gameToInstall.Title,
                "Download failed from all sources. This is likely due to a download link redirecting to an external website.",
                PackIconRemixIconKind.ErrorWarningLine);
            return false;
        }

        return true;
    }

    private async Task InitNotificationViewModel(MultiSourceGameSearchResultMetadataViewModel gameToInstall)
    {
        _notificationViewModel = new NotificationViewModel()
        {
            Title = gameToInstall.Title,
            Content = _installProgress!,
            OpenIcon = PackIconRemixIconKind.PlayLargeFill,
            CancelCommand = new AsyncRelayCommand(async () =>
            {
                await CancelInstall();
                if (_installProgress != null)
                {
                    _installProgress.Message = $"Download cancelled";
                    _installProgress.InstallStatus = InstallStatus.Canceled;
                }
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
                (dto) => dto?.Id != null && _installProgress?.InstallStatus == InstallStatus.Completed)
        };
        await _notificationService.AddNotificationAsync(_notificationViewModel);
    }

    private void DetectGameEngine(string installLocation, IGameMetadata dto)
    {
        var detectionResult = _engineDetector.Detect(installLocation);
        dto.ExecutablePath = detectionResult.ExecutablePath;
        dto.GameEngine = detectionResult.GameEngine;
        dto.SetupExecutable = detectionResult.SetupExecutablePath;
        dto.SetupExecutableArgs = detectionResult.SetupArgs;
        dto.CommunitySetupExecutable = detectionResult.CommunitySetupExecutablePath;
    }

    private async Task<int> SaveLinks(IMergedGameSearchResultMetadata allDetails, IGameMetadata dto, string? actualDownloadLink)
    {
        var linksToSave = new List<GameLinkDto>();
        foreach (var detail in allDetails.Sources)
        {
            linksToSave.Add(new GameLinkDto()
            {
                Link = detail.DownloadLink!,
                LinkType = LinkType.Download,
                GameId = dto.Id,
                BaseUrl = detail.BaseUrl,
                DisplayName = detail.SourceSiteDisplayName
            });

            if (detail.DetailsLink.IsNotNullOrWhiteSpace())
            {
                linksToSave.Add(new GameLinkDto()
                {
                    Link = detail.DetailsLink!,
                    LinkType = LinkType.Details,
                    GameId = dto.Id,
                    BaseUrl = detail.BaseUrl,
                    DisplayName = detail.SourceSiteDisplayName
                });
            }
            if (detail.HasReviews && detail.ReviewsLink.IsNotNullOrWhiteSpace())
            {
                linksToSave.Add(new GameLinkDto()
                {
                    Link = detail.ReviewsLink!,
                    LinkType = LinkType.Reviews,
                    GameId = dto.Id,
                    BaseUrl = detail.BaseUrl,
                    DisplayName = detail.SourceSiteDisplayName
                });
            }

            if (detail.HasWalkthrough && detail.WalkthroughLink.IsNotNullOrWhiteSpace())
            {
                linksToSave.Add(new GameLinkDto()
                {
                    Link = detail.WalkthroughLink!,
                    LinkType = LinkType.Walkthrough,
                    GameId = dto.Id,
                    BaseUrl = detail.BaseUrl,
                    DisplayName = detail.SourceSiteDisplayName
                });
            }
        }

        var savedLinks = await _gameLinkDataService.SaveLinks(linksToSave, _cancellationTokenSource.Token);

        var downloadLinkDto = savedLinks.FirstOrDefault(l => l.Link == actualDownloadLink);
        if (downloadLinkDto == null)
        {
            throw new InvalidOperationException($"Game link {actualDownloadLink} not saved to database!");
        }

        return downloadLinkDto.Id;
    }

    public async Task CancelInstall()
    {
        _logger.LogInformation("Install cancellation requested");
        await _cancellationTokenSource.CancelAsync();
        if (_installProgress != null)
        {
            _installProgress.Message = $"Download cancelled";
            _installProgress.InstallStatus = InstallStatus.Canceled;
        }
        _cancellationTokenSource = new CancellationTokenSource();
        _notificationViewModel?.IsOpenable = false;

        await AfterInstallCleanup();
        _logger.LogInformation("Installation canceled");
    }

    private async Task AfterInstallCleanup()
    {
        if (_downloadPath == null)
            return;
        if (Directory.Exists(_downloadPath))
            await _appFileOperations.DeleteDirectory(_downloadPath);
        else
            await _appFileOperations.DeleteFile(_downloadPath);

        if (_installedGameId != null && _installPath.IsNotNullOrWhiteSpace())
        {
            await _gameWithStatsService.Uninstall(_installedGameId.GetValueOrDefault());
        }

        _downloadPath = null;
        if (_notificationViewModel != null)
        {
            _notificationViewModel.IsDismissable = true;
            _notificationViewModel.IsCancelable = false;
        }
        _notificationViewModel = null;
        _installedGameId = null;
    }
}