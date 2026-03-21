using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using IconPacks.Avalonia.RemixIcon;
using TombLauncher.Configuration;
using TombLauncher.Core.PlatformSpecific;

namespace TombLauncher.ViewModels.Pages.Settings;

public partial class CompatibilitySettingsViewModel : SettingsSectionViewModelBase
{
    public CompatibilitySettingsViewModel(PageViewModel settingsPage, IPlatformSpecificFeatures platformFeatures)
        : base("COMPATIBILITY", settingsPage, PackIconRemixIconKind.EqualizerLine)
    {
        _platformFeatures = platformFeatures;
    }

    private readonly IPlatformSpecificFeatures _platformFeatures;

    [ObservableProperty] private string _winePath = string.Empty;
    [ObservableProperty] private string _winePrefix = string.Empty;
    [ObservableProperty] private string? _wineVersion;

    partial void OnWinePathChanged(string value) =>
        WineVersion = _platformFeatures.GetWineVersion(value);

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

    public override void ApplyTo(AppConfiguration userConfig)
    {
        userConfig.Compatibility.WinePath = WinePath;
        userConfig.Compatibility.WinePrefix = WinePrefix;
    }
}
