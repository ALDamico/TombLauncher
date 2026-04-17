namespace TombLauncher.Ai.Models;

public class AnnotatedChunk
{
    public required string SectionHeader { get; init; }
    public List<Chunk> Chunks { get; } = [];
    public SectionMetadata? SectionMetadata { get; init; }
}