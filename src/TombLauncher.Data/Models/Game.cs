using TombLauncher.Contracts.Enums;

namespace TombLauncher.Data.Models;

public class Game
{
    public int Id { get; set; }
    public required string Title { get; set; }
    public string? Author { get; set; }
    public DateTime? ReleaseDate { get; set; }
    public DateTime? InstallDate { get; set; }
    public GameEngine GameEngine { get; set; }
    public string? Setting { get; set; }
    public GameLength Length { get; set; }
    public GameDifficulty Difficulty { get; set; }
    public required string InstallDirectory { get; set; }
    public string? Description { get; set; }
    public Guid Guid { get; set; }
    public List<PlaySession> PlaySessions { get; set; } = [];
    public List<GameHashes> Hashes { get; set; } = [];
    public List<GameLink> Links { get; set; } = [];
    public List<FileBackup> FileBackups { get; set; } = [];
    public byte[]? TitlePic { get; set; }
    public string? AuthorFullName { get; set; }
    public bool IsInstalled { get; set; }
    public bool IsFavourite { get; set; }
    public bool IsCompleted { get; set; }
    public GameLink? InstalledFromLink { get; set; }
    public string? CompatibilityPrefixPath { get; set; }
    public int CompatibilityTool { get; set; }        // CompatibilityTool enum as int; 0 = Unspecified
    public string? CompatibilityToolPath { get; set; }
    public List<GameEnvironmentVariable> EnvironmentVariables { get; set; } = [];
}