using System.Threading.Tasks;
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
    }

    [ObservableProperty]
    public partial bool? SavegameBackupEnabled { get; set; }

    [ObservableProperty]
    public partial bool LimitNumberOfVersions { get; set; }

    [ObservableProperty]
    public partial int? NumberOfVersionsToKeep { get; set; }

    [ObservableProperty]
    public partial int SavegameProcessingDelay { get; set; }

    private readonly SettingsPageService _settingsService;
    
    [RelayCommand]
    private async Task SyncSavegamesInfo() => await _settingsService.SyncSavegames(SettingsPage);

    public override void ApplyTo(AppConfiguration userConfig)
    {
        userConfig.Savegames.BackupSavegamesEnabled = SavegameBackupEnabled;
        userConfig.Savegames.NumberOfVersionsToKeep = LimitNumberOfVersions ? NumberOfVersionsToKeep : null;
    }
}