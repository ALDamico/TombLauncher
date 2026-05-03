using LLama;
using LLama.Common;
using Microsoft.Extensions.Logging;
using TombLauncher.Ai.Abstractions;
using TombLauncher.Contracts.Progress;
using TombLauncher.Core.Dtos;

namespace TombLauncher.Ai.Services;

public class EmbeddingModelLoader
{
    public EmbeddingModelLoader(ModelDownloadService modelDownloadService, 
        IWeightsLoader weightsLoader, ILogger<EmbeddingModelLoader> logger, ILogger<LLamaEmbedder> embedderLogger)
    {
        _modelDownloadService = modelDownloadService;
        _weightsLoader = weightsLoader;
        _logger = logger;
        _embedderLogger = embedderLogger;
    }
    public bool IsLoaded { get; private set; }

    private LLamaEmbedder? _embedder;
    private readonly ModelDownloadService _modelDownloadService;
    private readonly IWeightsLoader _weightsLoader;
    private readonly ILogger<EmbeddingModelLoader> _logger;
    private readonly ILogger<LLamaEmbedder> _embedderLogger;

    public async Task<LLamaEmbedder> LoadEmbedder(IProgress<DownloadProgressInfo> progress, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Attempting to load embedder");
        if (IsLoaded && _embedder != null)
        {
            _logger.LogInformation("Embedder is already loaded.");
            return _embedder;
        }
        _logger.LogInformation("Checking if embedding model is available locally");
        var now = DateTime.Now;
        var modelMetadata = new AiModelMetadata()
        {
            Description = "Embedding model used by Tomb Launcher's AI",
            DownloadLink =
                "https://huggingface.co/nomic-ai/nomic-embed-text-v1.5-GGUF/resolve/main/nomic-embed-text-v1.5.f32.gguf",
            FileName = "nomic-embed-text-v1.5.f32.gguf",
            FriendlyName = "Nomic Embed Text v1.5",
            ModelId = "nomic-embed-text",
            Vendor = "Nomic AI",
        };

        if (!_modelDownloadService.IsModelDownloaded(modelMetadata))
        {
            _logger.LogInformation("Embedding model not found locally. Downloading it from {DownloadLink}", modelMetadata.DownloadLink);
            await _modelDownloadService.DownloadAsync(modelMetadata, progress, cancellationToken);
            _logger.LogInformation("Embedding model downloaded");
        }

        _logger.LogInformation("Loading embedding model");
        var modelParams = new ModelParams(_modelDownloadService.GetDestinationFilePath(modelMetadata));

        var weightsLoaderProgress = new Progress<float>(p => progress.Report(new DownloadProgressInfo()
            { TotalBytes = 100, BytesDownloaded = (long)p, StartDate = now }));

        var weights = await _weightsLoader.LoadWeightsAsync(modelParams, weightsLoaderProgress, cancellationToken);
        _logger.LogInformation("Embedding model loaded");
        IsLoaded = true;
        _embedder = new LLamaEmbedder(weights.Weights, modelParams, _embedderLogger);
        return _embedder;
    }
}