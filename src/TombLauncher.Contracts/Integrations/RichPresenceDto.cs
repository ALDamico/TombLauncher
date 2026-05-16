using TombLauncher.Contracts.Enums;

namespace TombLauncher.Contracts.Integrations;

public class RichPresenceDto
{
    public GameEngine Engine { get; init; }
    public string? WebsiteUrl { get; init; }
    public string? WebsiteCaption { get; init; }
    public string? LevelUrl { get; init; }
    public required string LevelName { get; init; }
    public required string AuthorName { get; init; }
}