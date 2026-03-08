namespace TombLauncher.Core.Dtos;

public class QuickStatsDto
{
    public int InstalledGamesCount { get; set; }
    public TimeSpan TotalPlayTime { get; set; }
    public int CompletedGamesCount { get; set; }
    public int FavouriteGamesCount { get; set; }
}
