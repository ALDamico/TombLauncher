namespace TombLauncher.Core.Dtos;

public class GameWithStatsDto
{
    public required GameMetadataDto GameMetadata { get; set; }
    public DateTime? LastPlayed { get; set; }
    public TimeSpan TotalPlayedTime { get; set; }
}