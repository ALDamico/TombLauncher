using TombLauncher.Data.Models;

namespace TombLauncher.Data.Dto;

public class GameLinkDto
{
    public int Id { get; set; }
    public int GameId { get; set; }
    public LinkType LinkType { get; set; }
    public string Link { get; set; }
}