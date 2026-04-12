namespace TombLauncher.Ai.Configuration;

public class KnowledgeBaseEmbedderConfiguration
{
    public required string EmbeddingModelUrl { get; set; }
    public required string ModelsPath { get; set; }
    public required string ModelFileName { get; set; }
    public required uint ContextLength { get; set; }
}