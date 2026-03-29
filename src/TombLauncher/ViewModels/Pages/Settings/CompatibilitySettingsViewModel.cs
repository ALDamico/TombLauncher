using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using IconPacks.Avalonia.RemixIcon;
using TombLauncher.Configuration;
using TombLauncher.Contracts.Enums;
using TombLauncher.Core.Dtos;
using TombLauncher.Core.PlatformSpecific;

namespace TombLauncher.ViewModels.Pages.Settings;

public partial class CompatibilitySettingsViewModel : SettingsSectionViewModelBase
{
    public CompatibilitySettingsViewModel(PageViewModel settingsPage, IPlatformSpecificFeatures platformFeatures)
        : base("COMPATIBILITY", settingsPage, PackIconRemixIconKind.EqualizerLine)
    {
        _platformFeatures = platformFeatures;

        AvailableProtonInstallations = new ObservableCollection<ProtonInstallationDto>(
            platformFeatures.FindAvailableProtonInstallations());
    }

    private readonly IPlatformSpecificFeatures _platformFeatures;

    // ─── Wine ────────────────────────────────────────────────────────────────

    [ObservableProperty] private string _winePath = string.Empty;
    [ObservableProperty] private string _winePrefix = string.Empty;
    [ObservableProperty] private string? _wineVersion;

    partial void OnWinePathChanged(string value) =>
        WineVersion = _platformFeatures.GetWineVersion(value);

    // ─── Proton ───────────────────────────────────────────────────────────────

    [ObservableProperty] private string? _protonVersion;
    [ObservableProperty] private string? _manualProtonPath;

    /// <summary>Proton installations discovered in steamapps.</summary>
    public ObservableCollection<ProtonInstallationDto> AvailableProtonInstallations { get; }

    private ProtonInstallationDto? _selectedProtonInstallation;
    public ProtonInstallationDto? SelectedProtonInstallation
    {
        get => _selectedProtonInstallation;
        set
        {
            if (SetProperty(ref _selectedProtonInstallation, value))
                ProtonVersion = value is null
                    ? _platformFeatures.GetProtonVersion(ManualProtonPath ?? "")
                    : _platformFeatures.GetProtonVersion(value.ExecutablePath);
        }
    }

    // ─── Tool selection ───────────────────────────────────────────────────────

    private CompatibilityTool _selectedTool = CompatibilityTool.Wine;
    public CompatibilityTool SelectedTool
    {
        get => _selectedTool;
        set
        {
            if (SetProperty(ref _selectedTool, value))
            {
                OnPropertyChanged(nameof(IsWine));
                OnPropertyChanged(nameof(IsProton));
                OnPropertyChanged(nameof(HasNoProtonInstallations));
            }
        }
    }

    public bool IsWine => SelectedTool == CompatibilityTool.Wine;
    public bool IsProton => SelectedTool == CompatibilityTool.Proton;

    /// <summary>True when no Proton installation was auto-detected → show manual path field.</summary>
    public bool HasNoProtonInstallations => AvailableProtonInstallations.Count == 0;

    // ─── Commands ─────────────────────────────────────────────────────────────

    public bool IsWineSupported => _platformFeatures.IsWineSupported;

    /// <summary>
    /// The Compatibility section is hidden on platforms where Wine is not supported
    /// (e.g. Windows), until additional compatibility tools (e.g. dgVoodoo) are added.
    /// </summary>
    public override bool HasContent => _platformFeatures.IsWineSupported;

    [RelayCommand]
    private void AutoDetectWine()
    {
        var found = _platformFeatures.FindWineExecutable();
        if (found != null)
            WinePath = found;
    }

    // ─── Persistence ─────────────────────────────────────────────────────────

    /// <summary>Returns the effective Proton executable path (selected or manual).</summary>
    private string? EffectiveProtonPath =>
        SelectedProtonInstallation?.ExecutablePath ?? ManualProtonPath;

    public override void ApplyTo(AppConfiguration userConfig)
    {
        userConfig.Compatibility.WinePath = WinePath;
        userConfig.Compatibility.WinePrefix = WinePrefix;
        userConfig.Compatibility.CompatibilityTool = SelectedTool;
        userConfig.Compatibility.ProtonPath = EffectiveProtonPath;
    }
}
