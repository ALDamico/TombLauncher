namespace TombLauncher.Ai.Configuration;

public interface IAiConfig
{
    bool IsAiEnabled { get; set; }
    string? KnowledgeBaseUrl { get; set; }
    string? KnowledgeBasePath { get; set; }
    double? GpuOffloadPercentage { get; set; }
    string? ModelName { get; set; }
    Dictionary<string, long>? ModelSizes { get; set; }
    string EmbeddingModelUrl { get; set; }
    string ModelsPath { get; set; }
    string EmbeddingModelFileName { get; set; }
    int EmbeddingContextLength { get; set; }
    string Endpoint { get; set; }
    string? ApiKey { get; set; }
}