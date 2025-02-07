using TombLauncher.Contracts.Enums;

namespace TombLauncher.Contracts.Downloaders;

public interface IGameMetadata
{
    int Id { get; set; }
    string Title { get; set; }
    string Author { get; set; }
    DateTime? ReleaseDate { get; set; }
    DateTime? InstallDate { get; set; }
    GameEngine GameEngine { get; set; }
    string Setting { get; set; }
    GameLength Length { get; set; }
    GameDifficulty Difficulty { get; set; }
    string InstallDirectory { get; set; }
    string ExecutablePath { get; set; }
    string Description { get; set; }
    Guid Guid { get; set; }
    byte[] TitlePic { get; set; }
    string AuthorFullName { get; set; }
    string UniversalLauncherPath { get; set; }
    bool IsInstalled { get; set; }
}