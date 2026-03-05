using TombLauncher.Contracts.Enums;

namespace TombLauncher.Core.Dtos;

public class GameLinkDto
{
    public int Id { get; set; }
    public int GameId { get; set; }
    public LinkType LinkType { get; set; }
    public required string Link { get; set; }
    public required string BaseUrl { get; set; }
    public required string DisplayName { get; set; }
}