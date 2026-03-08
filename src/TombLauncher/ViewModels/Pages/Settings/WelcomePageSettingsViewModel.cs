using CommunityToolkit.Mvvm.ComponentModel;

namespace TombLauncher.ViewModels.Pages.Settings;

public partial class WelcomePageSettingsViewModel : SettingsSectionViewModelBase
{
    public WelcomePageSettingsViewModel(PageViewModel settingsPage)
        : base("WELCOME_PAGE", settingsPage) { }

    [ObservableProperty] private bool _showQuickStats;
    [ObservableProperty] private bool _showQuickActions;
    [ObservableProperty] private bool _showRecentlyPlayed;
}
