using System.Net.Http.Json;
using System.Text.Json;
using TombLauncher.Ai.Abstractions;
using TombLauncher.Ai.Mappers;
using TombLauncher.Ai.Models;
using TombLauncher.Contracts.Ai;
using TombLauncher.Core.Dtos;

namespace TombLauncher.Ai.Services.AiBackends;

public class LmStudioBackendService : IAiBackendService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ModelMapper _modelMapper;

    public LmStudioBackendService(IHttpClientFactory httpClientFactory, ModelMapper modelMapper)
    {
        _httpClientFactory = httpClientFactory;
        _modelMapper = modelMapper;
    }

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
    };

    public bool SupportsModelDownload => true;
    public async Task<ServiceAvailabilityResponse> IsReachableAsync(string endpoint, string apiKey, CancellationToken ct)
    {
        try
        {
            _ = await IsModelInstalledAsync(endpoint, apiKey, "dummy", ct);
            return ServiceAvailabilityResponse.AvailableResponse;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            return ServiceAvailabilityResponse.NotAvailableResponse(ex.Message);
        }
    }

    public async Task<List<AiModelMetadata>> GetAvailableModelsAsync(string endpoint, string apiKey, CancellationToken ct)
    {
        var fullUri = GetFullUri(endpoint, "/api/v1/models");
        var modelResponse = await _httpClientFactory.CreateClient().GetFromJsonAsync<List<ModelInfo>>(fullUri, JsonOptions, ct);
        return _modelMapper.ToMetadataList(modelResponse ?? []);
    }

    public async Task<bool> IsModelInstalledAsync(string endpoint, string apiKey, string modelId, CancellationToken ct)
    {
        var models = await GetAvailableModelsAsync(endpoint, apiKey, ct);
        return models.Any(m => m.ModelId == modelId);
    }

    public async Task DownloadModelAsync(string endpoint, string apiKey, string modelId, IProgress<float> progress, CancellationToken ct)
    {
        var fullUri = GetFullUri(endpoint, "/api/v1/models/download");
        var body = new DownloadModelRequest()
        {
            Model = modelId
        };

        var response = await _httpClientFactory.CreateClient().PostAsJsonAsync(fullUri, body, JsonOptions, ct);

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException(HttpRequestError.ConnectionError,
                $"Endpoint {fullUri} responded with unexpected status code {response.StatusCode}",
                statusCode: response.StatusCode);
        }

        var downloadModelResponse = await response.Content.ReadFromJsonAsync<DownloadModelResponse>(JsonOptions, ct);
        if (downloadModelResponse == null)
        {
            throw new InvalidOperationException($"Response content for {fullUri} was empty");
        }
        switch (downloadModelResponse.Status)
        {
            case ModelDownloadStatus.Downloading:
            case ModelDownloadStatus.Paused:
                await PollDownload(endpoint, downloadModelResponse.JobId, progress, ct);
                break;
            case ModelDownloadStatus.AlreadyDownloaded:
            case ModelDownloadStatus.Completed:
                progress.Report(1);
                break;
            case ModelDownloadStatus.Failed:
                throw new InvalidOperationException($"Download of model {modelId} failed");
            default:
                throw new InvalidOperationException($"Unexpected download status: {downloadModelResponse.Status}");
        }
    }

    private async Task PollDownload(string endpoint, string? jobId, IProgress<float> progress, CancellationToken ct)
    {
        var statusUri = GetFullUri(endpoint, $"/api/v1/models/download/status/{jobId}");
        var client = _httpClientFactory.CreateClient();
        while (!ct.IsCancellationRequested)
        {
            var status = await client.GetFromJsonAsync<DownloadModelResponse>(statusUri, JsonOptions, ct);
            switch (status?.Status)
            {
                case ModelDownloadStatus.Completed:
                case ModelDownloadStatus.AlreadyDownloaded:
                    progress.Report(1f);
                    return;
                case ModelDownloadStatus.Failed:
                    throw new InvalidOperationException($"Download job {jobId} failed");
                default:
                    await Task.Delay(1000, ct);
                    break;
            }
        }
    }

    private Uri GetFullUri(string baseUrl, string endpoint)
    {
        return new UriBuilder(new Uri(baseUrl))
        {
            Path = endpoint
        }.Uri;
    }
}