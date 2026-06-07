using System.IO;
using CommunityToolkit.Mvvm.Input;
using IconPacks.Avalonia.RemixIcon;
using TombLauncher.Contracts.PlatformSpecific;
using TombLauncher.Core.PlatformSpecific;

namespace TombLauncher.ViewModels.Pages.Settings;

public partial class LoggingSettingsViewModel : SettingsSectionViewModelBase
{
    public LoggingSettingsViewModel(IPlatformSpecificFeatures platformSpecificFeatures, PageViewModel settingsPage) :
        base("LOGGING", settingsPage, PackIconRemixIconKind.FilePaper2Line)
    {
        _logsFolder = Path.Combine(platformSpecificFeatures.GetAppDataDirectory(), "Logs");
        _platformSpecificFeatures = platformSpecificFeatures;
    }

    private readonly string _logsFolder;
    private readonly IPlatformSpecificFeatures _platformSpecificFeatures;

    [RelayCommand]
    private void OpenLogsFolder()
    {
        _platformSpecificFeatures.OpenFolder(_logsFolder);
    }
}