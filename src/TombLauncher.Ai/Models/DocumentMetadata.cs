namespace TombLauncher.Ai.Models;

public class DocumentMetadata : MetadataBase
{
    public required string DocumentTitle { get; set; }
    public Dictionary<string, SectionMetadata> Sections { get; init; } = new();
    
}