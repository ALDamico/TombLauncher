namespace TombLauncher.Configuration.Sections;

public class WelcomePageConfig : IWelcomePageConfig
{
    public bool? ShowQuickStats { get; set; }
    public bool? ShowQuickActions { get; set; }
    public bool? ShowRecentlyPlayed { get; set; }
    public bool? ShowFavourites { get; set; }
    public int? RecentlyPlayedCount { get; set; }
    public int? FavouritesCount { get; set; }
    public bool? ShowRandomSuggestion { get; set; }
    public int? RandomGameMaxRerolls { get; set; }
}
