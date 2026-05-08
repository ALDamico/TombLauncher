namespace TombLauncher.Ai.Models;

public class SectionMetadata : MetadataBase
{
    public required string Header { get; set; }
    public string? AppVersionRange { get; set; }
    public Dictionary<string, string> EngineVersions { get; init; } = new();
}