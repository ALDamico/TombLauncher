using TombLauncher.Contracts.Progress;
using TombLauncher.Core.Dtos;
using TombLauncher.Core.Extensions;
using TombLauncher.Core.PlatformSpecific;

namespace TombLauncher.Ai.Services;

public class ModelDownloadService
{
    private readonly HttpClient _httpClient;
    private readonly IPlatformSpecificFeatures _platformSpecificFeatures;

    public ModelDownloadService(HttpClient httpClient, IPlatformSpecificFeatures platformSpecificFeatures)
    {
        _httpClient = httpClient;
        _platformSpecificFeatures = platformSpecificFeatures;
    }

    public async Task DownloadAsync(AiModelMetadata model, IProgress<DownloadProgressInfo> progress, CancellationToken cancellationToken)
    {
        var destinationFilePath = GetDestinationFilePath(model);
        Directory.CreateDirectory(Path.GetDirectoryName(destinationFilePath)!);
        await using var file = new FileStream(destinationFilePath, FileMode.Create);
        await _httpClient.DownloadAsync(model.DownloadLink, file, progress, cancellationToken);
    }

    public bool IsModelDownloaded(AiModelMetadata model) => File.Exists(GetDestinationFilePath(model));

    private string GetDestinationFilePath(AiModelMetadata model) =>
        Path.Combine(_platformSpecificFeatures.GetAppDataDirectory(), "Models", model.FileName);
}