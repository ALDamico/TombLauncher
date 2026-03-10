using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using IconPacks.Avalonia.RemixIcon;
using TombLauncher.Configuration;
using TombLauncher.Services;

namespace TombLauncher.ViewModels.Pages.Settings;

public partial class SavegameSettingsViewModel : SettingsSectionViewModelBase
{
    public SavegameSettingsViewModel(PageViewModel settingsPage, SettingsPageService settingsService) : base("SAVEGAMES", settingsPage, PackIconRemixIconKind.Save3Line)
    {
        _settingsService = settingsService;
        SyncSavegamesInfoCmd = new AsyncRelayCommand(SyncSavegamesInfo);
    }

    [ObservableProperty] private bool? _savegameBackupEnabled;
    [ObservableProperty] private bool _limitNumberOfVersions;
    [ObservableProperty] private int? _numberOfVersionsToKeep;
    [ObservableProperty] private int _savegameProcessingDelay;

    public ICommand SyncSavegamesInfoCmd { get; }
    private readonly SettingsPageService _settingsService;

    private async Task SyncSavegamesInfo() => await _settingsService.SyncSavegames(SettingsPage);

    public override void ApplyTo(AppConfiguration userConfig)
    {
        userConfig.Savegames.BackupSavegamesEnabled = SavegameBackupEnabled;
        userConfig.Savegames.NumberOfVersionsToKeep = LimitNumberOfVersions ? NumberOfVersionsToKeep : null;
    }
}