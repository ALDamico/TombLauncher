using TombLauncher.Contracts;
using TombLauncher.Contracts.Downloaders;
using TombLauncher.Contracts.Enums;

namespace TombLauncher.Core.Dtos;

public class GameMetadataDto : IGameMetadata
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Author { get; set; }
    public DateTime? ReleaseDate { get; set; }
    public DateTime? InstallDate { get; set; }
    public GameEngine GameEngine { get; set; }
    public string? Setting { get; set; }
    public GameLength Length { get; set; }
    public GameDifficulty Difficulty { get; set; }
    public string? InstallDirectory { get; set; }
    public string? ExecutablePath { get; set; }
    public string Description { get; set; } = string.Empty;
    public Guid Guid { get; set; }
    public byte[] TitlePic { get; set; } = Array.Empty<byte>();
    public string? AuthorFullName { get; set; }
    public bool IsInstalled { get; set; }
    public string? SetupExecutable { get; set; }
    public string? SetupExecutableArgs { get; set; }
    public string? CommunitySetupExecutable { get; set; }
    public bool IsFavourite { get; set; }
    public bool IsCompleted { get; set; }
    public string? InstalledFromSiteDisplayName { get; set; }
    public string? CompatibilityPrefixPath { get; set; }
    public CompatibilityTool CompatibilityTool { get; set; }
    public string? CompatibilityToolPath { get; set; }
    public List<IEnvironmentVariable> ExtraEnvVars { get; set; } = [];
}