using System.ComponentModel.DataAnnotations;
using CommunityToolkit.Mvvm.ComponentModel;
using IconPacks.Avalonia.RemixIcon;
using TombLauncher.Configuration;

namespace TombLauncher.ViewModels.Pages.Settings;

public partial class WelcomePageSettingsViewModel : SettingsSectionViewModelBase
{
    public WelcomePageSettingsViewModel(PageViewModel settingsPage)
        : base("WELCOME_PAGE", settingsPage, PackIconRemixIconKind.Home4Line) { }

    [ObservableProperty]
    public partial bool ShowQuickStats { get; set; }

    [ObservableProperty]
    public partial bool ShowQuickActions { get; set; }

    [ObservableProperty]
    public partial bool ShowRecentlyPlayed { get; set; }

    [ObservableProperty]
    public partial bool ShowFavourites { get; set; }

    [ObservableProperty]
    public partial int RecentlyPlayedCount { get; set; }

    [ObservableProperty]
    public partial int FavouritesCount { get; set; }

    [ObservableProperty]
    public partial bool ShowRandomSuggestion { get; set; }

    [ObservableProperty]
    [Range(1, 15, ErrorMessage = "Allowed values: 1-15")]
    public partial int MaxRerolls { get; set; }

    public override void ApplyTo(AppConfiguration userConfig)
    {
        userConfig.WelcomePage.ShowQuickStats = ShowQuickStats;
        userConfig.WelcomePage.ShowQuickActions = ShowQuickActions;
        userConfig.WelcomePage.ShowRecentlyPlayed = ShowRecentlyPlayed;
        userConfig.WelcomePage.ShowFavourites = ShowFavourites;
        userConfig.WelcomePage.RecentlyPlayedCount = RecentlyPlayedCount;
        userConfig.WelcomePage.FavouritesCount = FavouritesCount;
        userConfig.WelcomePage.ShowRandomSuggestion = ShowRandomSuggestion;
        userConfig.WelcomePage.RandomGameMaxRerolls = MaxRerolls;
    }
}
