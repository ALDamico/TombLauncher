using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using JamSoft.AvaloniaUI.Dialogs.MsgBox;
using Microsoft.Extensions.Logging;
using TombLauncher.Contracts.EngineDetectors;
using TombLauncher.Core.Extensions;
using TombLauncher.Core.PlatformSpecific;
using TombLauncher.Data.Database.Services;
using TombLauncher.Extensions;
using TombLauncher.Localization.Extensions;
using TombLauncher.Mappers;
using TombLauncher.ViewModels;
using TombLauncher.ViewModels.Pages;

namespace TombLauncher.Services;

public class LaunchOptionsService : IViewService
{
    public LaunchOptionsService(
        ViewServiceContext viewContext,
        GameDataService gameDataService,
        IEngineDetector engineDetector,
        ILogger<LaunchOptionsService> logger,
        LaunchOptionsMapper mapper,
        IPlatformSpecificFeatures platformSpecificFeatures)
    {
        ViewContext = viewContext;
        _gameDataService = gameDataService;
        _engineDetector = engineDetector;
        _logger = logger;
        _mapper = mapper;
        _platformSpecificFeatures = platformSpecificFeatures;
    }

    public ViewServiceContext ViewContext { get; }
    private readonly GameDataService _gameDataService;
    private readonly IEngineDetector _engineDetector;
    private readonly ILogger<LaunchOptionsService> _logger;
    private readonly LaunchOptionsMapper _mapper;
    private readonly IPlatformSpecificFeatures _platformSpecificFeatures;
    public NavigationManager NavigationManager => ViewContext.NavigationManager;

    public async Task LoadAsync(LaunchOptionsViewModel vm, GameMetadataViewModel game)
    {
        try
        {
            vm.InitFromGame(game);
        }
        catch (IOException ex)
        {
            _logger.LogError(ex, "Error reading install directory for game {GameId}", game.Id);
            await ViewContext.PopupService.ShowLocalized("ERROR", "LAUNCH_OPTIONS_EXECUTABLE_READ_ERROR",
                MsgBoxButton.Ok, MsgBoxImage.Error);
            await GoBackAsync();
        }
    }

    public void AutoDetect(LaunchOptionsViewModel vm)
    {
        if (vm.AvailableExecutables == null || vm.InstallDirectory == null) return;

        var result = _engineDetector.Detect(vm.InstallDirectory);
        vm.SelectedEngine = result.GameEngine;
        vm.GameExecutable = vm.AvailableExecutables.Contains(result.ExecutablePath ?? "")
            ? result.ExecutablePath
            : vm.AvailableExecutables.Count > 0 ? vm.AvailableExecutables[0] : null;

        if (result.SetupExecutablePath != null && vm.AvailableExecutables.Contains(result.SetupExecutablePath))
        {
            vm.SupportsSetup = true;
            vm.SetupExecutable = result.SetupExecutablePath;
            vm.SetupArgs = result.SetupArgs;
        }
        else
        {
            vm.SupportsSetup = false;
        }

        if (result.CommunitySetupExecutablePath != null && vm.AvailableExecutables.Contains(result.CommunitySetupExecutablePath))
        {
            vm.SupportsCustomSetup = true;
            vm.CustomSetupExecutable = result.CommunitySetupExecutablePath;
        }
        else
        {
            vm.SupportsCustomSetup = false;
        }
    }

    public async Task SaveAsync(LaunchOptionsViewModel vm)
    {
        try
        {
            if (NavigationManager.CurrentPage is PageViewModel currentPage)
            {
                using (currentPage.BusyScope("SAVING_LAUNCH_OPTIONS".GetLocalizedString()))
                {
                    var dto =  _mapper.ToDto(vm);
                    await _gameDataService.UpdateLaunchOptions(dto);
                }
            }
        }
        catch (Exception e)
        {
            _logger.LogWarning(e, "Error saving launch options.");
        }

        await GoBackAsync();
    }

    public Task GoBackAsync() => NavigationManager.GoBack();

    public ObservableCollection<string> GetAvailableExecutables(GameMetadataViewModel game)
    {
        var installDirectory = game.InstallDirectory ?? "";
        if (installDirectory.IsNullOrWhiteSpace())
            return [];
        return Directory.GetFiles(installDirectory, "*.exe", _platformSpecificFeatures.GetEnumerationOptions())
            .Select(p => Path.GetRelativePath(installDirectory, p))
            .ToObservableCollection();
    }
}
