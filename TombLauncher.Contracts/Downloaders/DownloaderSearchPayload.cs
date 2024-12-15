using TombLauncher.Contracts.Enums;

namespace TombLauncher.Contracts.Downloaders;

public class DownloaderSearchPayload
{
    public string LevelName { get; set; }
    public string AuthorName { get; set; }
    public GameEngine? GameEngine { get; set; }
    public GameDifficulty? GameDifficulty { get; set; }
    public GameLength? Duration { get; set; }
    public int Rating { get; set; }
    public string Setting { get; set; }
}