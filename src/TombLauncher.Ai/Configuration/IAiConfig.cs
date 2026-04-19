namespace TombLauncher.Ai.Configuration;

public interface IAiConfig
{
    bool IsAiEnabled { get; set; }
    string? KnowledgeBaseUrl { get; set; }
    string? KnowledgeBasePath { get; set; }
    double? GpuOffloadPercentage { get; set; }
    string? ModelName { get; set; }
    Dictionary<string, long>? ModelSizes { get; set; }
}