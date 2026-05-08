namespace TombLauncher.Ai.Models;

public class DocumentMetadata : MetadataBase
{
    public required string DocumentTitle { get; set; }
    public List<SectionMetadata> Sections { get; init; } = new();
}