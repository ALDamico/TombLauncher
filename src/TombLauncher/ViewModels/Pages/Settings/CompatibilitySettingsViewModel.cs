using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using IconPacks.Avalonia.RemixIcon;
using TombLauncher.Configuration;
using TombLauncher.Contracts.Enums;
using TombLauncher.Contracts.PlatformSpecific;
using TombLauncher.Contracts.Settings;

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

    [ObservableProperty]
    public partial string WinePath { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string CompatibilityPrefixPath { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string? WineVersion { get; set; }

    partial void OnWinePathChanged(string value) =>
        WineVersion = _platformFeatures.GetWineVersion(value);

    // ─── Proton ───────────────────────────────────────────────────────────────

    [ObservableProperty]
    public partial string? ProtonVersion { get; set; }

    [ObservableProperty]
    public partial string? ManualProtonPath { get; set; }

    /// <summary>Proton installations discovered in steamapps.</summary>
    public ObservableCollection<ProtonInstallationDto> AvailableProtonInstallations { get; }

    public ProtonInstallationDto? SelectedProtonInstallation
    {
        get;
        set
        {
            if (SetProperty(ref field, value))
                ProtonVersion = value is null
                    ? _platformFeatures.GetProtonVersion(ManualProtonPath ?? "")
                    : _platformFeatures.GetProtonVersion(value.ExecutablePath);
        }
    }

    // ─── Tool selection ───────────────────────────────────────────────────────

    public CompatibilityTool SelectedTool
    {
        get;
        set
        {
            if (SetProperty(ref field, value))
            {
                OnPropertyChanged(nameof(IsWine));
                OnPropertyChanged(nameof(IsProton));
                OnPropertyChanged(nameof(HasNoProtonInstallations));
            }
        }
    } = CompatibilityTool.Wine;

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
        userConfig.Compatibility.CompatibilityPrefixPath = CompatibilityPrefixPath;
        userConfig.Compatibility.CompatibilityTool = SelectedTool;
        userConfig.Compatibility.ProtonPath = EffectiveProtonPath;
    }
}
