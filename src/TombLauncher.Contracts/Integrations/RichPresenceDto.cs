namespace TombLauncher.Contracts.Integrations;

public class RichPresenceDto
{
    public required string Title { get; init; }
    public string? WebsiteUrl { get; init; }
    public string? WebsiteCaption { get; init; }
    public string? LevelUrl { get; init; }
    public string? LevelCaption { get; init; }
    public required string LevelName { get; init; }
    public string? ScreenshotUrl { get; init; }
    public required string AuthorName { get; init; }
    public DateTime StartTimestamp { get; init; }
}