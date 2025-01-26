using CommunityToolkit.Mvvm.ComponentModel;
using TombLauncher.Localization.Extensions;

namespace TombLauncher.ViewModels.Pages.Settings;

public partial class SavegameSettingsViewModel : SettingsSectionViewModelBase
{
    public SavegameSettingsViewModel() : base("SAVEGAMES")
    {
    }

    [ObservableProperty] private bool _savegameBackupEnabled;
    [ObservableProperty] private bool _limitNumberOfVersions;
    [ObservableProperty] private int? _numberOfVersionsToKeep;
}