using System.ClientModel;
using OpenAI;
using OpenAI.Models;
using TombLauncher.Ai.Abstractions;
using TombLauncher.Ai.Mappers;
using TombLauncher.Core.Dtos;

namespace TombLauncher.Ai.Services;

public class OllamaBackendService : IAiBackendService
{
    private readonly ModelMapper _modelMapper;

    public OllamaBackendService(ModelMapper modelMapper)
    {
        _modelMapper = modelMapper;
    }
    public bool SupportsModelDownload => false;
    public async Task<bool> IsReachableAsync(string endpoint, string apiKey, CancellationToken ct)
    {
        try
        {
            _ = await GetModelClient(endpoint, apiKey).GetModelsAsync(ct);
            return true;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            return false;
        }
    }

    public async Task<List<AiModelMetadata>> GetAvailableModelsAsync(string endpoint, string apiKey, CancellationToken ct)
    {
        var models = await GetModelClient(endpoint, apiKey).GetModelsAsync(ct);
        var modelCollection = models.Value;
        return _modelMapper.ToMetadataList(modelCollection);
    }

    public async Task<bool> IsModelInstalledAsync(string endpoint, string apiKey, string modelId, CancellationToken ct)
    {
        try
        {
            var model = await GetModelClient(endpoint, apiKey).GetModelAsync(modelId, ct);
            return model?.Value != null;
        }
        catch (ClientResultException ex) when (ex.Status == 404)
        {
            return false;
        }
    }

    public Task DownloadModelAsync(string endpoint, string apiKey, string modelId, IProgress<float> progress, CancellationToken ct)
    {
        throw new NotSupportedException("The Ollama backend does not support downloading models. Manage them in your Ollama instance.");
    }

    private OpenAIModelClient GetModelClient(string endpoint, string apiKey)
    {
        var openAiClient = new OpenAIClient(new ApiKeyCredential(apiKey),
            new OpenAIClientOptions() { Endpoint = new Uri(endpoint) });
        return openAiClient.GetOpenAIModelClient();
    }
}