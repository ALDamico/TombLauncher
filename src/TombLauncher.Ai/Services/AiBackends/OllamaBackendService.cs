using System.ClientModel;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OpenAI;
using OpenAI.Models;
using TombLauncher.Ai.Abstractions;
using TombLauncher.Ai.Mappers;
using TombLauncher.Contracts.Ai;
using TombLauncher.Core.Dtos;
using TombLauncher.Core.Extensions;

namespace TombLauncher.Ai.Services.AiBackends;

public class OllamaBackendService : IAiBackendService
{
    private readonly ModelMapper _modelMapper;

    public OllamaBackendService(ModelMapper modelMapper)
    {
        _modelMapper = modelMapper;
    }
    
    public bool SupportsModelDownload => false;
    public async Task<ServiceAvailabilityResponse> IsReachableAsync(string endpoint, string apiKey, CancellationToken ct)
    {
        try
        {
            var clientResult = await GetModelClient(endpoint, apiKey).GetModelsAsync(ct);
            if (clientResult.Value.Count > 0) 
                return ServiceAvailabilityResponse.AvailableResponse;
            var data = await clientResult.GetRawResponse().BufferContentAsync(ct);
            var jObject = JsonConvert.DeserializeObject<JObject>(data.ToString());
            if (jObject == null)
                return ServiceAvailabilityResponse.AvailableResponse;
            if (jObject.TryGetValue("error", out var errorMessage))
            {
                return ServiceAvailabilityResponse.NotAvailableResponse(errorMessage.Value<string>() ?? "No reason given");
            }
            return ServiceAvailabilityResponse.AvailableResponse;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            return ServiceAvailabilityResponse.NotAvailableResponse(ex.Message);
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
        if (apiKey.IsNullOrWhiteSpace())
        {
            apiKey = "tomb_launcher";
        }
        var openAiClient = new OpenAIClient(new ApiKeyCredential(apiKey),
            new OpenAIClientOptions() { Endpoint = new Uri(GetFullEndpoint(endpoint)) });
        return openAiClient.GetOpenAIModelClient();
    }

    private string GetFullEndpoint(string endpoint)
    {
        endpoint = endpoint.TrimEnd('/');
        if (!endpoint.EndsWith("v1"))
            endpoint += "/v1";
        return endpoint;
    }
}