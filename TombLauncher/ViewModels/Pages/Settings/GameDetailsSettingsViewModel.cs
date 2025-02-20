using CommunityToolkit.Mvvm.ComponentModel;

namespace TombLauncher.ViewModels.Pages.Settings;

public partial class GameDetailsSettingsViewModel : SettingsSectionViewModelBase
{
    public GameDetailsSettingsViewModel(PageViewModel settingsPage) : base("GAME DETAILS", settingsPage)
    {
    }

    [ObservableProperty] private bool _askForConfirmationBeforeWalkthrough;
    [ObservableProperty] private bool _useInternalViewerIfAvailable;
}