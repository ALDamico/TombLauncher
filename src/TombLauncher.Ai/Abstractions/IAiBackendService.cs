using TombLauncher.Contracts.Ai;
using TombLauncher.Core.Dtos;

namespace TombLauncher.Ai.Abstractions;

public interface IAiBackendService
{
    bool SupportsModelDownload { get; }
    Task<ServiceAvailabilityResponse> IsReachableAsync(string endpoint, string apiKey, CancellationToken ct);
    Task<List<AiModelMetadata>> GetAvailableModelsAsync(string endpoint, string apiKey, CancellationToken ct);
    Task<bool> IsModelInstalledAsync(string endpoint, string apiKey, string modelId, CancellationToken ct);
    Task DownloadModelAsync(string endpoint, string apiKey, string modelId, IProgress<float> progress, CancellationToken ct);
}