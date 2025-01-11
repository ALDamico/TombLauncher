namespace TombLauncher.Core.Dtos;

public class GameStatisticsDto
{
    public string Title { get; set; }
    public DateTime? LastPlayed { get; set; }
    public DateTime? LastPlayedEnd { get; set; }
    public TimeSpan Duration => LastPlayedEnd.GetValueOrDefault() - LastPlayed.GetValueOrDefault();
    public uint TotalSessions { get; set; }
}