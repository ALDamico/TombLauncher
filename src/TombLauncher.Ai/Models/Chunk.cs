using TombLauncher.Contracts.Ai;

namespace TombLauncher.Ai.Models;

public class Chunk
{
    public int Id { get; set; }
    public string? DocumentTitle { get; set; }
    public string? SectionTitle { get; set; }
    public string? ChunkText { get; set; }
    public float[] Embedding { get; init; } = new float[AiConstants.EmbeddingSize];
    public int? MetadataId { get; set; }
    public double Distance { get; set; }
}