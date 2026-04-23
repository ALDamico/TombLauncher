namespace TombLauncher.Ai.Configuration;

public class AiConfig : IAiConfig
{
    public bool IsAiEnabled { get; set; }
    public string? KnowledgeBaseUrl { get; set; }
    public string? KnowledgeBasePath { get; set; }
    public double? GpuOffloadPercentage { get; set; }
    public string? ModelName { get; set; }
    public Dictionary<string, long>? ModelSizes { get; set; }
    public string EmbeddingModelUrl { get; set; } = null!;
    public string ModelsPath { get; set; } = null!;
    public string EmbeddingModelFileName { get; set; } = null!;
    public uint EmbeddingContextLength { get; set; }
}