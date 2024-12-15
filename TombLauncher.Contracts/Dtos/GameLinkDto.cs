using TombLauncher.Contracts.Enums;

namespace TombLauncher.Contracts.Dtos;

public class GameLinkDto
{
    public int Id { get; set; }
    public int GameId { get; set; }
    public LinkType LinkType { get; set; }
    public string Link { get; set; }
}