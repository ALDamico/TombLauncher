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
        var urlToFetch = model.DownloadLink;
        var destinationFilePath = GetDestinationFilePath(model);
        await using var file = new FileStream(destinationFilePath, FileMode.Create);
        await _httpClient.DownloadAsync(urlToFetch, file, progress, cancellationToken);
    }

    private string GetDestinationFilePath(AiModelMetadata model)
    {
        var destinationDirectory = Path.Combine(_platformSpecificFeatures.GetAppDataDirectory(), "Models");
        Directory.CreateDirectory(destinationDirectory);
        var destinationFilePath = Path.Combine(destinationDirectory, model.FileName);
        return destinationFilePath;
    }
}