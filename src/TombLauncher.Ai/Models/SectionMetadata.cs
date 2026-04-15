namespace TombLauncher.Ai.Models;

public class SectionMetadata : MetadataBase
{
    public string? AppVersionRange { get; set; }
    public Dictionary<string, string> EngineVersions { get; init; } = new();
}