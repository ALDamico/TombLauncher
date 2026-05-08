using TombLauncher.Contracts.Enums;

namespace TombLauncher.Ai.Configuration;

public interface IAiConfig
{
    bool IsAiEnabled { get; set; }
    string? KnowledgeBaseUrl { get; set; }
    string? KnowledgeBasePath { get; set; }
    string? ModelId { get; set; }
    string? EmbeddingModelId { get; set; }
    string Endpoint { get; set; }
    string? ApiKey { get; set; }
    AiBackendType BackendType { get; set; }
}