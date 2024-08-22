using System;

namespace TombLauncher.Dto;

public class GameMetadataDto
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string Author { get; set; }
    public DateTime? ReleaseDate { get; set; }
    public DateTime? InstallDate { get; set; }
    public string GameEngine { get; set; }
    public string Setting { get; set; }
    public string Length { get; set; }
    public string Difficulty { get; set; }
    public string InstallDirectory { get; set; }
    public string ExecutablePath { get; set; }
    public string Description { get; set; }
}