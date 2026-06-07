using TombLauncher.Contracts.Enums;

namespace TombLauncher.Contracts.Downloaders;

public interface IGameMetadataLite
{
    int Id { get; set; }
    string Title { get; set; }
    string? Author { get; set; }
    DateTime? ReleaseDate { get; set; }
    DateTime? InstallDate { get; set; }
    GameEngine GameEngine { get; set; }
    string? Setting { get; set; }
    GameLength Length { get; set; }
    GameDifficulty Difficulty { get; set; }
    string? InstallDirectory { get; set; }
    string? ExecutablePath { get; set; }
    string Description { get; set; }
    Guid Guid { get; set; }
    
    string? AuthorFullName { get; set; }
    bool IsInstalled { get; set; }
    string? SetupExecutable { get; set; }
    string? SetupExecutableArgs { get; set; }
    string? CommunitySetupExecutable { get; set; }
    bool IsFavourite { get; set; }
    bool IsCompleted { get; set; }
    string? InstalledFromSiteDisplayName { get; set; }
    string? CompatibilityPrefixPath { get; set; }
    CompatibilityTool CompatibilityTool { get; set; }
    string? CompatibilityToolPath { get; set; }
    string? TitlePicUrl { get; set; }
    string? InstalledFromLink { get; set; }
    bool EnableBorderlessFix { get; set; }
}