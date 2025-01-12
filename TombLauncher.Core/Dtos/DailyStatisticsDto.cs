namespace TombLauncher.Core.Dtos;

public class DailyStatisticsDto
{
    public DateTime Date { get; set; }
    public int DifferentGamesPlayed { get; set; }
    public TimeSpan AverageGameDuration { get; set; }
    public TimeSpan TotalPlayTime { get; set; }
}