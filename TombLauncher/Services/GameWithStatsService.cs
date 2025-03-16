using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using CommunityToolkit.Mvvm.DependencyInjection;
using JamSoft.AvaloniaUI.Dialogs;
using Microsoft.Extensions.Logging;
using TombLauncher.Contracts.Localization;
using TombLauncher.Core.Dtos;
using TombLauncher.Core.Extensions;
using TombLauncher.Core.Navigation;
using TombLauncher.Core.PlatformSpecific;
using TombLauncher.Core.Savegames;
using TombLauncher.Core.Utils;
using TombLauncher.Data.Database.UnitOfWork;
using TombLauncher.Localization.Extensions;
using TombLauncher.ViewModels;
using TombLauncher.ViewModels.Pages;

namespace TombLauncher.Services;

public class GameWithStatsService : IViewService
{
    public GameWithStatsService()
    {
        _gamesUnitOfWork = Ioc.Default.GetRequiredService<GamesUnitOfWork>();
        LocalizationManager = Ioc.Default.GetRequiredService<ILocalizationManager>();
        NavigationManager = Ioc.Default.GetRequiredService<NavigationManager>();
        MessageBoxService = Ioc.Default.GetRequiredService<IMessageBoxService>();
        DialogService = Ioc.Default.GetRequiredService<IDialogService>();
        var settingsService = Ioc.Default.GetRequiredService<SettingsService>();
        var savegameSettings = settingsService.GetSavegameSettings(null);
        _backupEnabled = savegameSettings.SavegameBackupEnabled.GetValueOrDefault();
        if (_backupEnabled)
        {
            _numberOfSavesToKeep = savegameSettings.NumberOfVersionsToKeep;
        }

        _mapper = Ioc.Default.GetRequiredService<MapperConfiguration>().CreateMapper();
        _logger = Ioc.Default.GetRequiredService<ILogger<GameWithStatsService>>();
        _savegameService = Ioc.Default.GetRequiredService<SavegameService>();
        _winePath = settingsService.GetWinePath();
        _platformSpecificFeatures = Ioc.Default.GetRequiredService<IPlatformSpecificFeatures>();
    }

    private SavegameHeaderProcessor _headerProcessor;
    private readonly IMapper _mapper;

    private readonly GamesUnitOfWork _gamesUnitOfWork;
    public ILocalizationManager LocalizationManager { get; }
    public NavigationManager NavigationManager { get; }
    public IMessageBoxService MessageBoxService { get; }
    public IDialogService DialogService { get; }
    private FileSystemWatcher _watcher;
    private readonly bool _backupEnabled;
    private readonly int? _numberOfSavesToKeep;
    private readonly ILogger<GameWithStatsService> _logger;
    private readonly SavegameService _savegameService;
    private readonly string _winePath;
    private DateTime? _startDate;
    private IPlatformSpecificFeatures _platformSpecificFeatures;

    public async Task OpenGame(GameWithStatsViewModel game)
    {
        var gameDetailsViewModel = new GameDetailsViewModel(game);
        await NavigationManager.NavigateTo(gameDetailsViewModel);
    }

    public async Task OpenGame(int gameId)
    {
        var game = await GetGameById(gameId);
        await OpenGame(game);
    }

    public void PlayGame(GameWithStatsViewModel game)
    {
        var currentPage = NavigationManager.GetCurrentPage();
        currentPage.SetBusy(LocalizationManager.GetLocalizedString("Starting GAMENAME", game.GameMetadata.Title));
        InitFileSystemWatcher(game);
        //watcher.Created += WatcherOnCreated;

        LaunchProcess(game, game.GameMetadata.ExecutablePath, true);
    }

    private void InitFileSystemWatcher(GameWithStatsViewModel game)
    {
        if (_watcher != null)
        {
            _watcher.Changed -= WatcherOnCreatedOrChanged;
        }
        try
        {
            _watcher = new FileSystemWatcher(game.GameMetadata.InstallDirectory, "save*.*")
            {
                IncludeSubdirectories = true,
                EnableRaisingEvents = true,
                InternalBufferSize = 8192 * 32,
                NotifyFilter = _platformSpecificFeatures.GetSavegameWatcherNotifyFilters()
            };
            _headerProcessor = Ioc.Default.GetRequiredService<SavegameHeaderProcessor>();
            _headerProcessor.SavegameHeaderReader = _savegameService.GetHeaderReader(game.GameMetadata.GameEngine);
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
        _headerProcessor.EnqueueFileName(e.FullPath);
    }

    public async Task PlayGame(int gameId)
    {
        var gameViewModel = await GetGameById(gameId);
        await OpenGame(gameViewModel);
        PlayGame(gameViewModel);
    }

    private async Task<GameWithStatsViewModel> GetGameById(int gameId)
    {
        var game = await _gamesUnitOfWork.GetGameWithStats(gameId);
        return _mapper.Map<GameWithStatsViewModel>(game);
    }

    public bool CanPlayGame(GameWithStatsViewModel game)
    {
        return game.GameMetadata.IsInstalled;
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
        var currentPage = NavigationManager.GetCurrentPage();
        currentPage.ClearBusy();
    }

    private async Task OnGameExited(GameWithStatsViewModel game, Process process)
    {
        Console.WriteLine("Exited!");
        if (process.ExitCode != 0)
        {
            _logger.LogWarning("Setup process exited with exit code {ExitCode}", process.ExitCode);
        }
        var currentPage = NavigationManager.GetCurrentPage();
        try
        {
            currentPage.SetBusy(true, "Saving play session...".GetLocalizedString());
            var gameMetadataDto = _mapper.Map<GameMetadataDto>(game.GameMetadata);
            _gamesUnitOfWork.AddPlaySessionToGame(gameMetadataDto, _startDate.GetValueOrDefault(), process.ExitTime);
            currentPage.SetBusy("Backing up savegames...".GetLocalizedString());
            var filesToProcess = _headerProcessor.ProcessedFiles;

            foreach (var file in filesToProcess)
            {
                file.Md5 = Md5Utils.ComputeMd5Hash(file.Data).GetAwaiter().GetResult();
            }

            filesToProcess = filesToProcess.DistinctBy(f => f.Md5).ToList();

            if (_backupEnabled)
            {
                _gamesUnitOfWork.BackupSavegames(game.GameMetadata.Id, game.GameMetadata.GameEngine, filesToProcess, _numberOfSavesToKeep);
                _headerProcessor.ClearProcessedFiles();
                try
                {
                    _headerProcessor?.Dispose();
                    _watcher?.Dispose();
                }
                catch (ObjectDisposedException)
                {
                    // Shit happens
                }
                finally
                {
                    _headerProcessor = null;
                    _watcher = null;
                }
            }

            await _gamesUnitOfWork.Save();

            NavigationManager.RequestRefresh();
        }
        finally
        {
            currentPage.ClearBusy();
        }
    }

    public void LaunchSetup(GameWithStatsViewModel game)
    {
        var currentPage = NavigationManager.GetCurrentPage();
        currentPage.SetBusy(
            LocalizationManager.GetLocalizedString("Launching setup for GAMENAME", game.GameMetadata.Title));
        LaunchProcess(game, game.GameMetadata.SetupExecutable, false, game.GameMetadata.SetupExecutableArgs);
    }

    public void LaunchCommunitySetup(GameWithStatsViewModel game)
    {
        var currentPage = NavigationManager.GetCurrentPage();
        currentPage.SetBusy(
            LocalizationManager.GetLocalizedString("Launching community patch setup for GAMENAME", game.GameMetadata.Title));
        LaunchProcess(game, game.GameMetadata.CommunitySetupExecutable);
    }

    private void LaunchProcess(GameWithStatsViewModel game, string executable, bool trackPlayTime = false, string arguments = null)
    {
        var platformSpecificFeatures = Ioc.Default.GetRequiredService<IPlatformSpecificFeatures>();
        var executableFileNameOnly = Path.GetFileName(executable);

        var workingDirectory = Path.GetDirectoryName(Path.Combine(game.GameMetadata.InstallDirectory, executable));
        // Process.StartTime is not supported under Linux. We instead keep track of the start time with this field. 
        _startDate = DateTime.Now;

        var process = new Process()
        {
            StartInfo = platformSpecificFeatures.GetGameLaunchStartInfo(executableFileNameOnly, arguments ?? "", _winePath, workingDirectory),
            EnableRaisingEvents = true
        };
        if (trackPlayTime)
        {
            process.Exited += async (sender, _) => await OnGameExited(game, sender as Process);
        }
        else
        {
            process.Exited += (sender, _) => OnSetupExited(sender as Process);
        }

        process.ErrorDataReceived += (_, args) => _logger.LogError("Process error: {StandardError}", args.Data);
        process.OutputDataReceived +=
            (_, args) => _logger.LogInformation("Process output: {StandardOutput}", args.Data);

        process.Start();
    }

    public async Task ToggleFavourite(GameWithStatsViewModel gameWithStatsViewModel)
    {
        var metadata = _mapper.Map<GameMetadataDto>(gameWithStatsViewModel.GameMetadata);
        metadata.IsFavourite = !metadata.IsFavourite;
        await _gamesUnitOfWork.UpsertGame(metadata);
        gameWithStatsViewModel.GameMetadata.IsFavourite = metadata.IsFavourite;
    }

    public async Task ToggleCompleted(GameWithStatsViewModel gameWithStatsViewModel)
    {
        var metadata = _mapper.Map<GameMetadataDto>(gameWithStatsViewModel.GameMetadata);
        metadata.IsCompleted = !metadata.IsCompleted;
        await _gamesUnitOfWork.UpsertGame(metadata);
        gameWithStatsViewModel.GameMetadata.IsCompleted = metadata.IsCompleted;
    }
    
    public bool CanUninstall(GameMetadataViewModel metadataViewModel)
    {
        return metadataViewModel.IsInstalled;
    }

    public async Task Uninstall(string installDir, int gameId)
    {
        NavigationManager.GetCurrentPage().SetBusy("Uninstalling...");
        Directory.Delete(installDir, true);
        _gamesUnitOfWork.MarkGameAsUninstalled(gameId);
        await _gamesUnitOfWork.Save();
        NavigationManager.GoBack();
    }
}