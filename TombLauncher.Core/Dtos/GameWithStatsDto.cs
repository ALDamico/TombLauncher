namespace TombLauncher.Core.Dtos;

public class GameWithStatsDto
{
    public GameMetadataDto GameMetadata { get; set; }
    public DateTime? LastPlayed { get; set; }
    public TimeSpan TotalPlayedTime { get; set; }
}