namespace TombLauncher.Configuration.Sections;

public interface IWelcomePageConfig
{
    bool? ShowQuickStats { get; }
    bool? ShowQuickActions { get; }
    bool? ShowRecentlyPlayed { get; }
    bool? ShowFavourites { get; }
    int? RecentlyPlayedCount { get; }
    int? FavouritesCount { get; }
    bool? ShowRandomSuggestion { get; }
    int? RandomGameMaxRerolls { get; }
}
