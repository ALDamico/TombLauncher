using System;
using System.Collections.Generic;

namespace TombLauncher.Models;

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
    public Guid Guid { get; set; }
    public List<PlaySession> PlaySessions { get; set; }
    public List<GameHashes> Hashes { get; set; }
    public List<GameLink> Links { get; set; }
    public byte[] TitlePic { get; set; }
    public string AuthorFullName { get; set; }
}