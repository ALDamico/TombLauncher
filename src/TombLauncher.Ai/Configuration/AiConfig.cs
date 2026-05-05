using TombLauncher.Contracts.Enums;

namespace TombLauncher.Ai.Configuration;

public class AiConfig : IAiConfig
{
    public bool IsAiEnabled { get; set; }
    public string? KnowledgeBaseUrl { get; set; }
    public string? KnowledgeBasePath { get; set; }
    public string? ModelId { get; set; }
    public string? EmbeddingModelId { get; set; }
    public string Endpoint { get; set; } = "http://localhost:11434/v1";
    public string? ApiKey { get; set; }
    public AiBackendType BackendType { get; set; }
}