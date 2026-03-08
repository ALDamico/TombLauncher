using System.ComponentModel.DataAnnotations;
using CommunityToolkit.Mvvm.ComponentModel;
using IconPacks.Avalonia.RemixIcon;

namespace TombLauncher.ViewModels.Pages.Settings;

public partial class WelcomePageSettingsViewModel : SettingsSectionViewModelBase
{
    public WelcomePageSettingsViewModel(PageViewModel settingsPage)
        : base("WELCOME_PAGE", settingsPage, PackIconRemixIconKind.Home4Line) { }

    [ObservableProperty] private bool _showQuickStats;
    [ObservableProperty] private bool _showQuickActions;
    [ObservableProperty] private bool _showRecentlyPlayed;
    [ObservableProperty] private bool _showFavourites;
    [ObservableProperty] private int _recentlyPlayedCount;
    [ObservableProperty] private int _favouritesCount;
    [ObservableProperty] private bool _showRandomSuggestion;

    [ObservableProperty]
    [Range(1, 15, ErrorMessage = "Allowed values: 1-15")]
    private int _maxRerolls;
}
