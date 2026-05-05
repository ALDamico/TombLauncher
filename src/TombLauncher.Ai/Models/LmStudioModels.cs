namespace TombLauncher.Ai.Models;

internal class DownloadModelRequest
{
    public required string Model { get; set; }
    public string? Quantization { get; set; }
}

internal class DownloadModelResponse
{
    public string? JobId { get; set; }
    public ModelDownloadStatus? Status { get; set; }
    public DateTime? CompletedAt { get; set; }
    public long TotalSizeBytes { get; set; }
    public DateTime? StartedAt { get; set; }
}

internal enum ModelDownloadStatus
{
    Downloading,
    Paused,
    Completed,
    Failed,
    AlreadyDownloaded
}

internal class LmStudioModelResponse
{
    public List<ModelInfo> Models { get; set; } = [];
}

internal class ModelInfo
{
    public required string Type { get; set; }
    public required string Publisher { get; set; }
    public required string Key { get; set; }
    public string? DisplayName { get; set; }
    public string? Architecture { get; set; }
    public QuantizationInfo? Quantization { get; set; }
    public long SizeBytes { get; set; }
    public string? ParamsString { get; set; }
    public int MaxContextLength { get; set; }
    public string? Format { get; set; }
    public string? Description { get; set; }
    public List<string> Variants { get; set; } = [];
    public string? SelectedVariant { get; set; }
    public ModelCapabilities? Capabilities { get; set; }
}

internal class QuantizationInfo
{
    public string? Name { get; set; }
    public int? BitsPerWeight { get; set; }
}

internal class ModelCapabilities
{
    public bool Vision { get; set; }
    public bool TrainedFoToolUse { get; set; }
    public ReasoningCapability? Reasoning { get; set; }
}

internal enum ReasoningModes
{
    Off,
    On,
    Low,
    Medium,
    High
}

internal class ReasoningCapability
{
    public List<string> AllowedOptions { get; set; } = [];
    public string? Default { get; set; }
}

internal class ModelConfig
{
    public int ContextLength { get; set; }
    public int? EvalBatchSize { get; set; }
    public int? Parallel { get; set; }
    public bool? FlashAttention { get; set; }
    public int? NumExperts { get; set; }
    public bool? OffloadKvCacheToGpu { get; set; }
}

internal class ModelInstance
{
    public required string Id { get; set; }
    public required ModelConfig Config { get; set; }
}
