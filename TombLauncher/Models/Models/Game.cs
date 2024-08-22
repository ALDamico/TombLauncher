using System;

namespace TombLauncher.Models.Models;

public class Game
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string Author { get; set; }
    public DateTime? ReleaseDate { get; set; }
    public DateTime? InstallDate { get; set; }
    public GameEngine GameEngine { get; set; }
    public string Setting { get; set; }
    public GameLength Length { get; set; }
    public GameDifficulty Difficulty { get; set; }
    public string InstallDirectory { get; set; }
    public string ExecutablePath { get; set; }
    public string Description { get; set; }
}