using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TombLauncher.Contracts.Enums;
using TombLauncher.Core.Extensions;
using TombLauncher.Core.PlatformSpecific;
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
        GameExecutable = AvailableExecutables.FirstOrDefault(e => e == game.ExecutablePath);
        SetupArgs = game.SetupExecutableArgs;
        SetupExecutable = AvailableExecutables.FirstOrDefault(e => e == game.SetupExecutable);
        CustomSetupExecutable = AvailableExecutables.FirstOrDefault(e => e == game.CommunitySetupExecutable);
        CompatibilityPrefixPath = game.CompatibilityPrefixPath;

        if (SetupExecutable.IsNotNullOrWhiteSpace()) SupportsSetup = true;
        if (CustomSetupExecutable.IsNotNullOrWhiteSpace()) SupportsCustomSetup = true;
    }

    // ── Properties ────────────────────────────────────────────────────────────
    public string? InstallDirectory { get; private set; }
    [ObservableProperty] private ObservableCollection<string>? _availableExecutables;
    [ObservableProperty] private string? _gameExecutable;
    [ObservableProperty] private ObservableCollection<EnumViewModel<GameEngine>> _availableEngines = null!;
    [ObservableProperty] private GameEngine _selectedEngine;
    [ObservableProperty] private string? _setupExecutable;
    [ObservableProperty] private string? _setupArgs;
    [ObservableProperty] private bool _supportsSetup;
    [ObservableProperty] private bool _supportsCustomSetup;
    [ObservableProperty] private string? _customSetupExecutable;
    [ObservableProperty] private string? _compatibilityPrefixPath;
    public bool IsWineSupported { get; }

    // ── Commands ──────────────────────────────────────────────────────────────

    [RelayCommand]
    private void AutoDetect() => _service.AutoDetect(this);

    protected override bool CanSave() => true;

    protected override async Task SaveInner() => await _service.SaveAsync(this);

    protected override void Cancel() => _ = _service.GoBackAsync();
}
