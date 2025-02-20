using TombLauncher.Contracts.Enums;

namespace TombLauncher.Core.Dtos;

public class GameLinkDto
{
    public int Id { get; set; }
    public int GameId { get; set; }
    public LinkType LinkType { get; set; }
    public string Link { get; set; }
    public string BaseUrl { get; set; }
    public string DisplayName { get; set; }
}