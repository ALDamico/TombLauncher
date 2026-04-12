namespace TombLauncher.Ai.Configuration;

public interface IAiConfig
{
    bool IsAiEnabled { get; set; }
    string? KnowledgeBaseUrl { get; set; }
    int? GpuLayerCount { get; set; }
    string? ModelName { get; set; }
}