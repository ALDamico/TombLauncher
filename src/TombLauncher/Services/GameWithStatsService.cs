using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JamSoft.AvaloniaUI.Dialogs.MsgBox;
using Microsoft.Extensions.Logging;
using TombLauncher.Configuration;
using TombLauncher.Contracts.Navigation;
using TombLauncher.Core.Extensions;
using TombLauncher.Contracts.Enums;
using TombLauncher.Contracts.Integrations;
using TombLauncher.Core.Dtos;
using TombLauncher.Core.Launchers;
using TombLauncher.Core.PlatformSpecific;
using TombLauncher.Core.Savegames;
using TombLauncher.Core.Utils;
using TombLauncher.Data.Database.Repositories;
using TombLauncher.Data.Database.Services;
using TombLauncher.Extensions;
using TombLauncher.Integrations.Discord;
using TombLauncher.Localization.Extensions;
using TombLauncher.Mappers;
using TombLauncher.Utils;
using TombLauncher.ViewModels;
using TombLauncher.ViewModels.Pages;

namespace TombLauncher.Services;

public class GameWithStatsService : IViewService, IDisposable
{
    public GameWithStatsService(
        ViewServiceContext viewContext,
        GameDataService gameDataService,
        PlaySessionDataService playSessionDataService,
        ISavegameRepository savegameRepository,
        ISettingsProvider settingsProvider,
        ILogger<GameWithStatsService> logger,
        ISavegameHeaderProvider headerProvider,
        IPlatformSpecificFeatures platformSpecificFeatures,
        SavegameHeaderProcessor headerProcessor,
        IAppConfiguration appConfiguration,
        Func<IGameLauncher> gameLauncherFactory,
        GameMetadataMapper mapper, 
        DiscordRichPresenceService discordService)
    {
        ViewContext = viewContext;
        _gameDataService = gameDataService;
        _playSessionDataService = playSessionDataService;
        _savegameRepository = savegameRepository;
        var savegameSettings = settingsProvider.GetSavegameSettings();
        _backupEnabled = savegameSettings.IsBackupEnabled;
        if (_backupEnabled)
        {
            _numberOfSavesToKeep = savegameSettings.NumberOfVersionsToKeep;
        }

        _logger = logger;
        _headerProvider = headerProvider;
        _gameLauncherFactory = gameLauncherFactory;
        _globalCompatibilityPrefixPath = appConfiguration.Compatibility.CompatibilityPrefixPath;
        _appConfiguration = appConfiguration;
        _platformSpecificFeatures = platformSpecificFeatures;
        _headerProcessor = headerProcessor;
        _mapper = mapper;
        _discordService = discordService;
        _isDiscordSharingEnabled = appConfiguration.Integrations.SharePlaySessionsOnDiscord ?? false;
    }

    public ViewServiceContext ViewContext { get; }
    private SavegameHeaderProcessor? _headerProcessor;
    private readonly GameMetadataMapper _mapper;
    private readonly DiscordRichPresenceService _discordService;
    private readonly bool _isDiscordSharingEnabled;

    private readonly GameDataService _gameDataService;
    private readonly PlaySessionDataService _playSessionDataService;
    private readonly ISavegameRepository _savegameRepository;
    public NavigationManager NavigationManager => ViewContext.NavigationManager;
    private FileSystemWatcher? _watcher;
    private readonly bool _backupEnabled;
    private readonly int? _numberOfSavesToKeep;
    private readonly ILogger<GameWithStatsService> _logger;
    private readonly ISavegameHeaderProvider _headerProvider;
    private readonly Func<IGameLauncher> _gameLauncherFactory;
    private readonly string? _globalCompatibilityPrefixPath;
    private readonly IAppConfiguration _appConfiguration;
    private DateTime? _startDate;
    private readonly IPlatformSpecificFeatures _platformSpecificFeatures;

    public async Task OpenGame(GameWithStatsViewModel? game)
    {
        await NavigationManager.NavigateTo<GameDetailsViewModel>(game);
    }

    public async Task OpenGame(int gameId)
    {
        var game = await GetGameById(gameId);
        await OpenGame(game);
    }

    public void PlayGame(GameWithStatsViewModel game)
    {
        var currentPage = NavigationManager.CurrentPage as INavigationTarget;
        currentPage?.SetBusy("STARTING_GAMENAME".GetLocalizedString(game.GameMetadata.Title));
        InitFileSystemWatcher(game);
        _headerProcessor?.Start();

        if (game.GameMetadata.ExecutablePath != null)
        {
            if (_isDiscordSharingEnabled)
            {
                _discordService.UpdateStatus(new RichPresenceDto()
                {
                    DiscordAppId = _appConfiguration.Integrations.DiscordAppId ?? "",
                    LevelName = game.GameMetadata.Title, 
                    Title = $"Playing {game.GameMetadata.Title}",
                    State = "Powered by Tomb Launcher",
                    AuthorName = game.GameMetadata.Author,
                    WebsiteUrl = "https://tomblauncher.app", 
                    WebsiteCaption = "Try Tomb Launcher",
                    LevelUrl = game.GameMetadata.InstalledFromLink,
                    LevelCaption = $"Try {game.GameMetadata.Title}",
                    ScreenshotUrl = game.GameMetadata.TitlePicUrl,
                });
            }
            LaunchProcess(game, game.GameMetadata.ExecutablePath, true);
        }
    }

    private void InitFileSystemWatcher(GameWithStatsViewModel game)
    {
        if (_watcher != null)
        {
            _watcher.Changed -= WatcherOnCreatedOrChanged;
        }

        try
        {
            _watcher = new FileSystemWatcher(game.GameMetadata.InstallDirectory ?? string.Empty, "save*.*")
            {
                IncludeSubdirectories = true,
                EnableRaisingEvents = true,
                InternalBufferSize = 8192 * 32,
                NotifyFilter = _platformSpecificFeatures.GetSavegameWatcherNotifyFilters()
            };
            if (_headerProcessor != null)
                _headerProcessor.SavegameHeaderReader = _headerProvider.GetHeaderReader(game.GameMetadata.GameEngine);
            _watcher.Changed += WatcherOnCreatedOrChanged;
        }
        catch
        {
            _watcher?.Dispose();
            throw;
        }
    }

    private void WatcherOnCreatedOrChanged(object sender, FileSystemEventArgs e)
    {
        _headerProcessor?.EnqueueFileName(e.FullPath);
    }

    public async Task PlayGame(int gameId)
    {
        var gameViewModel = await GetGameById(gameId);
        await OpenGame(gameViewModel);
        PlayGame(gameViewModel!);
    }

    private async Task<GameWithStatsViewModel?> GetGameById(int gameId)
    {
        var game = await _gameDataService.GetGameWithStats(gameId);
        return _mapper.ToViewModel(game, this);
    }

    public bool CanPlayGame(GameWithStatsViewModel game)
    {
        return game.GameMetadata.IsInstalled && game.GameMetadata.GameEngine != GameEngine.Unknown;
    }

    public bool CanLaunchSetup(GameWithStatsViewModel game)
    {
        return game.GameMetadata.SetupExecutable.IsNotNullOrWhiteSpace();
    }

    public bool CanLaunchCommunitySetup(GameWithStatsViewModel game)
    {
        return game.GameMetadata.CommunitySetupExecutable.IsNotNullOrWhiteSpace();
    }

    private void OnSetupExited(Process process)
    {
        if (process.ExitCode != 0)
        {
            _logger.LogWarning("Setup process exited with exit code {ExitCode}", process.ExitCode);
        }
        else
        {
            _logger.LogInformation("Setup process exited without issue");
        }

        var currentPage = NavigationManager.CurrentPage as INavigationTarget;
        currentPage?.ClearBusy();
    }

    private async Task<PlaySessionCrashDto?> ReadGameCrash(GameMetadataViewModel game, int exitCode,
        string standardOutput, string standardError)
    {
        var crashFiles = AppUtils.GetLogFiles(game.InstallDirectory!, _startDate!.Value);
        var playSessionCrashDto = new PlaySessionCrashDto()
        {
            ExitCode = exitCode,
            StdOut = standardOutput,
            StdErr = standardError
        };
        foreach (var crashFile in crashFiles)
        {
            var dto = new CrashFileDto(Path.GetFileName(crashFile), await File.ReadAllTextAsync(crashFile));
            playSessionCrashDto.CrashFiles.Add(dto);
        }

        return playSessionCrashDto;
    }

    private async Task OnGameExited(GameWithStatsViewModel game, Process process, string standardOutput,
        string standardError)
    {
        _logger.LogInformation("Play session for game {GameTitle} (ID: {GameId}) ended.", game.GameMetadata.Title,
            game.GameMetadata.Id);
        if (_isDiscordSharingEnabled)
        {
            _discordService.EndPlaySession();
        }
        var exitCode = process.ExitCode;
        var errorOccurred = false;
        PlaySessionCrashDto? playSessionCrashDto = null;
        var currentPage = NavigationManager.CurrentPage as INavigationTarget;

        if (process.ExitCode != 0)
        {
            _logger.LogWarning("Game process exited with exit code {ExitCode}", process.ExitCode);
            currentPage?.SetBusy("SAVING_PLAY_SESSION_ERROR_DATA");
            playSessionCrashDto = await ReadGameCrash(game.GameMetadata, exitCode, standardOutput, standardError);
        }

        try
        {
            using (currentPage?.BusyScope("SAVING_PLAY_SESSION".GetLocalizedString()))
            {
                var gameMetadataDto = _mapper.ToDto(game.GameMetadata);
                await _playSessionDataService.AddPlaySessionToGame(gameMetadataDto, _startDate.GetValueOrDefault(),
                    process.ExitTime, playSessionCrashDto);
                currentPage?.SetBusy("BACKING_UP_SAVEGAMES".GetLocalizedString());
                var filesToProcess = _headerProcessor?.ProcessedFiles ?? new();

                foreach (var file in filesToProcess)
                {
                    file.Md5 = CryptoUtils.ComputeMd5Hash(file.Data);
                }

                filesToProcess = filesToProcess.DistinctBy(f => f.Md5).ToList();

                if (_backupEnabled)
                {
                    _savegameRepository.BackupSavegames(game.GameMetadata.Id, game.GameMetadata.GameEngine,
                        filesToProcess, _numberOfSavesToKeep);
                    _headerProcessor?.ClearProcessedFiles();
                }

                errorOccurred = _headerProcessor?.ErrorOccurred ?? false;
                CleanupWatcherAndProcessor();


                await _savegameRepository.Save();
            }
        }
        finally
        {
            if (errorOccurred)
            {
                await ViewContext.PopupService.ShowLocalized("Savegame parse error",
                    "An error occurred while processing a savegame. Savegames have not been backed up.",
                    MsgBoxButton.Ok, MsgBoxImage.Error);
            }
        }
    }

    public void LaunchSetup(GameWithStatsViewModel game)
    {
        var currentPage = NavigationManager.CurrentPage as INavigationTarget;
        currentPage?.SetBusy("LAUNCHING_SETUP_FOR_GAMENAME".GetLocalizedString(game.GameMetadata.Title));
        if (game.GameMetadata.SetupExecutable != null)
        {
            LaunchProcess(game, game.GameMetadata.SetupExecutable, false, game.GameMetadata.SetupExecutableArgs);
        }
    }

    public void LaunchCommunitySetup(GameWithStatsViewModel game)
    {
        var currentPage = NavigationManager.CurrentPage as INavigationTarget;
        currentPage?.SetBusy("LAUNCHING_COMMUNITY_PATCH_SETUP_FOR_GAMENAME".GetLocalizedString(game.GameMetadata.Title));
        if (game.GameMetadata.CommunitySetupExecutable != null)
        {
            LaunchProcess(game, game.GameMetadata.CommunitySetupExecutable);
        }
    }

    private void LaunchProcess(GameWithStatsViewModel game, string executable, bool trackPlayTime = false,
        string? arguments = null)
    {
        var executableFileNameOnly = Path.GetFileName(executable);
        string workingDirectory = string.Empty;
        if (game.GameMetadata.InstallDirectory != null)
        {
            workingDirectory = Path.GetDirectoryName(Path.Combine(game.GameMetadata.InstallDirectory, executable)) ??
                               string.Empty;
        }

        // Process.StartTime is not supported under Linux. We instead keep track of the start time with this field. 
        _startDate = DateTime.Now;

        var rawPrefix = game.GameMetadata.CompatibilityPrefixPath.IsNotNullOrWhiteSpace()
            ? game.GameMetadata.CompatibilityPrefixPath
            : _globalCompatibilityPrefixPath;
        var winePrefix = rawPrefix != null ? _platformSpecificFeatures.ExpandPath(rawPrefix) : null;

        var perGameTool = game.GameMetadata.CompatibilityTool;
        IGameLauncher launcher = perGameTool == CompatibilityTool.Automatic
            ? _gameLauncherFactory()
            : perGameTool switch
            {
                CompatibilityTool.Proton => new ProtonGameLauncher(
                    game.GameMetadata.CompatibilityToolPath.IsNotNullOrWhiteSpace()
                        ? game.GameMetadata.CompatibilityToolPath!
                        : _appConfiguration.Compatibility.ProtonPath ?? ""),
                CompatibilityTool.WindowsNative => new WindowsGameLauncher(),
                CompatibilityTool.LinuxNative => new LinuxGameLauncher(),
                _ => new WineGameLauncher(
                    game.GameMetadata.CompatibilityToolPath.IsNotNullOrWhiteSpace()
                        ? game.GameMetadata.CompatibilityToolPath!
                        : _appConfiguration.Compatibility.WinePath ?? "wine")
            };

        var extraEnvVars = game.GameMetadata.ExtraEnvVars.Count > 0
            ? game.GameMetadata.ExtraEnvVars.ToDictionary(e => e.VariableName, e => e.VariableValue)
            : null;

        var process = new Process()
        {
            StartInfo = launcher.GetLaunchStartInfo(new GameLaunchContext
            {
                ExecutableFileName = executableFileNameOnly,
                Arguments = arguments ?? "",
                WorkingDirectory = workingDirectory,
                PrefixPath = winePrefix,
                ExtraEnvVars = extraEnvVars
            }),
            EnableRaisingEvents = true
        };

        var stdoutBuilder = new StringBuilder();
        var stderrBuilder = new StringBuilder();

        // Redirect streams only when UseShellExecute=false (Linux/Wine); not available on Windows with UseShellExecute=true.
        if (!process.StartInfo.UseShellExecute)
        {
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.OutputDataReceived += (_, args) =>
            {
                if (args.Data != null) stdoutBuilder.AppendLine(args.Data);
            };
            process.ErrorDataReceived += (_, args) =>
            {
                if (args.Data != null) stderrBuilder.AppendLine(args.Data);
            };
        }

        if (trackPlayTime)
        {
            process.Exited += async (sender, _) =>
            {
                var p = (Process)sender!;
                LogProcessOutputOnError(p, stdoutBuilder, stderrBuilder);
                await OnGameExited(game, p, stdoutBuilder.ToString(), stderrBuilder.ToString());
            };
        }
        else
        {
            process.Exited += (sender, _) =>
            {
                var p = (Process)sender!;
                LogProcessOutputOnError(p, stdoutBuilder, stderrBuilder);
                OnSetupExited(p);
            };
        }

        process.Start();

        if (!process.StartInfo.UseShellExecute)
        {
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
        }
    }

    private void LogProcessOutputOnError(Process process, StringBuilder stdout, StringBuilder stderr)
    {
        if (process.ExitCode == 0) return;
        var stdoutStr = stdout.ToString();
        var stderrStr = stderr.ToString();
        if (!string.IsNullOrWhiteSpace(stdoutStr))
            _logger.LogWarning("Process stdout:{NewLine}{StdOut}", Environment.NewLine, stdoutStr);
        if (!string.IsNullOrWhiteSpace(stderrStr))
            _logger.LogWarning("Process stderr:{NewLine}{StdErr}", Environment.NewLine, stderrStr);
    }

    public async Task ToggleFavourite(GameWithStatsViewModel gameWithStatsViewModel)
    {
        var metadata = _mapper.ToDto(gameWithStatsViewModel.GameMetadata);
        metadata.IsFavourite = !metadata.IsFavourite;
        await _gameDataService.UpsertGame(metadata);
        gameWithStatsViewModel.GameMetadata.IsFavourite = metadata.IsFavourite;
    }

    public async Task ToggleCompleted(GameWithStatsViewModel gameWithStatsViewModel)
    {
        var metadata = _mapper.ToDto(gameWithStatsViewModel.GameMetadata);
        metadata.IsCompleted = !metadata.IsCompleted;
        await _gameDataService.UpsertGame(metadata);
        gameWithStatsViewModel.GameMetadata.IsCompleted = metadata.IsCompleted;
    }

    public bool CanUninstall(GameMetadataViewModel metadataViewModel)
    {
        return metadataViewModel.IsInstalled;
    }

    public async Task Uninstall(int gameId)
    {
        var game = await _gameDataService.GetGameWithStats(gameId);
        var currentPage = NavigationManager.CurrentPage as INavigationTarget;
        if (currentPage is PageViewModel pageVm)
        {
            using (pageVm.BusyScope("UNINSTALLING".GetLocalizedString(game.GameMetadata.Title)))
            {
                var installDir = game.GameMetadata.InstallDirectory;
                if (installDir != null)
                {
                    Directory.Delete(installDir, true);
                }

                await _gameDataService.MarkGameAsUninstalled(gameId);
            }
        }

        await NavigationManager.GoBack();
    }

    private void CleanupWatcherAndProcessor()
    {
        try
        {
            _headerProcessor?.Dispose();
            _watcher?.Dispose();
        }
        catch (ObjectDisposedException)
        {
            // Already disposed
        }
        finally
        {
            _headerProcessor = null;
            _watcher = null;
        }
    }

    public void Dispose()
    {
        CleanupWatcherAndProcessor();
    }
}