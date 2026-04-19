namespace TombLauncher.Ai.Configuration;

public class AiConfig : IAiConfig
{
    public bool IsAiEnabled { get; set; }
    public string? KnowledgeBaseUrl { get; set; }
    public string? KnowledgeBasePath { get; set; }
    public double? GpuOffloadPercentage { get; set; }
    public string? ModelName { get; set; }
}