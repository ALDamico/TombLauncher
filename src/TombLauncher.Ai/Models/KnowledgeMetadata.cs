namespace TombLauncher.Ai.Models;

public class KnowledgeMetadata
{
    public int Id { get; set; }
    public string? AppVersionRange { get; set; }
    public string? EngineVersion { get; set; }
    public string? AppliesTo { get; set; }
    public string? Platforms { get; set; }
    public string? Source { get; set; }
}