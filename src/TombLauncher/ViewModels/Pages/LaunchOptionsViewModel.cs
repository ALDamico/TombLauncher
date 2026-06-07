using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TombLauncher.Contracts.Enums;
using TombLauncher.Contracts.PlatformSpecific;
using TombLauncher.Core.Dtos;
using TombLauncher.Core.Extensions;
using TombLauncher.Services;
using TombLauncher.Utils;

namespace TombLauncher.ViewModels.Pages;

public partial class LaunchOptionsViewModel : PageViewModel
{
    private readonly LaunchOptionsService _service;

    public LaunchOptionsViewModel(LaunchOptionsService service, IPlatformSpecificFeatures platformFeatures)
    {
        _service = service;
        AvailableEngines = EnumUtils.GetEnumViewModels<GameEngine>().ToObservableCollection();
        SelectedEngine = GameEngine.Unknown;
        IsWineSupported = platformFeatures.IsWineSupported;
    }

    public override async Task OnNavigatedTo(object parameter)
    {
        if (parameter is GameMetadataViewModel game)
        {
            await _service.LoadAsync(this, game);
        }
    }

    // ── Game identity ──────────────────────────────────────────────────────────
    public int GameId { get; private set; }

    // Called by LaunchOptionsService during Load
    internal void InitFromGame(GameMetadataViewModel game)
    {
        GameId = game.Id;
        AvailableExecutables = _service.GetAvailableExecutables(game);

        SelectedEngine = game.GameEngine;
        InstallDirectory = game.InstallDirectory;
        GameExecutable = AvailableExecutables.FirstOrDefault(e => e == game.ExecutablePath);
        SetupArgs = game.SetupExecutableArgs;
        SetupExecutable = AvailableExecutables.FirstOrDefault(e => e == game.SetupExecutable);
        CustomSetupExecutable = AvailableExecutables.FirstOrDefault(e => e == game.CommunitySetupExecutable);
        CompatibilityPrefixPath = game.CompatibilityPrefixPath;
        CompatibilityTool = game.CompatibilityTool;
        CompatibilityToolPath = game.CompatibilityToolPath;
        ExtraEnvVars = game.ExtraEnvVars;

        if (SetupExecutable.IsNotNullOrWhiteSpace()) SupportsSetup = true;
        if (CustomSetupExecutable.IsNotNullOrWhiteSpace()) SupportsCustomSetup = true;
    }

    // ── Properties ────────────────────────────────────────────────────────────
    public string? InstallDirectory { get; private set; }
    [ObservableProperty]
    public partial ObservableCollection<string>? AvailableExecutables { get; set; }

    [ObservableProperty]
    public partial string? GameExecutable { get; set; }

    [ObservableProperty]
    public partial ObservableCollection<EnumViewModel<GameEngine>> AvailableEngines { get; set; }

    [ObservableProperty]
    public partial GameEngine SelectedEngine { get; set; }

    [ObservableProperty]
    public partial string? SetupExecutable { get; set; }

    [ObservableProperty]
    public partial string? SetupArgs { get; set; }

    [ObservableProperty]
    public partial bool SupportsSetup { get; set; }

    [ObservableProperty]
    public partial bool SupportsCustomSetup { get; set; }

    [ObservableProperty]
    public partial string? CustomSetupExecutable { get; set; }

    [ObservableProperty]
    public partial string? CompatibilityPrefixPath { get; set; }

    [ObservableProperty]
    public partial CompatibilityTool CompatibilityTool { get; set; }

    [ObservableProperty]
    public partial string? CompatibilityToolPath { get; set; }
    public List<EnvironmentVariableDto> ExtraEnvVars { get; set; } = [];
    public bool IsWineSupported { get; }

    // ── Commands ──────────────────────────────────────────────────────────────

    [RelayCommand]
    private void AutoDetect() => _service.AutoDetect(this);

    protected override bool CanSave() => true;

    protected override async Task SaveInner() => await _service.SaveAsync(this);

    protected override void Cancel() => _ = _service.GoBackAsync();
}
