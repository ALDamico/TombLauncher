using Microsoft.Extensions.AI;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using TombLauncher.Ai.Models;
using TombLauncher.Ai.Utils;

namespace TombLauncher.Ai.Services;

public class EmbedderService : IHostedService
{
    private readonly IHostApplicationLifetime _lifetime;
    private readonly ILogger<EmbedderService> _logger;
    private readonly IEmbeddingGenerator<string, Embedding<float>> _embedder;
    private readonly KnowledgeBaseWriter _knowledgeBaseWriter;
    private const string InputPath = "./Input";
    private const string IgnoreDir = "_ignored";

    public EmbedderService(IHostApplicationLifetime lifetime, ILogger<EmbedderService> logger,
        IEmbeddingGenerator<string, Embedding<float>> embedder, KnowledgeBaseWriter knowledgeBaseWriter)
    {
        _lifetime = lifetime;
        _logger = logger;
        _embedder = embedder;
        _knowledgeBaseWriter = knowledgeBaseWriter;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var files = GetFilesToProcess(InputPath);
        var allChunks = new List<AnnotatedChunk>();

        foreach (var file in files)
        {
            var fileContentTask = File.ReadAllTextAsync(file, cancellationToken);
            var metadataTask = ReadDocumentMetadata(file, cancellationToken);
            await Task.WhenAll(fileContentTask, metadataTask);
            var fileContent = fileContentTask.Result;
            var metadata = metadataTask.Result;
            _logger.LogInformation("Computing embedding for {File}", file);

            var annotatedChunks = await DocumentChunker.GetAnnotatedChunks(fileContent, _embedder, metadata, cancellationToken);
            allChunks.AddRange(annotatedChunks);
        }
        _logger.LogInformation("Saving embeddings");
        await _knowledgeBaseWriter.WriteChunks(allChunks, cancellationToken);

        _lifetime.StopApplication();
    }

    private static async Task<DocumentMetadata> ReadDocumentMetadata(string fileName, CancellationToken cancellationToken)
    {
        var directory = Path.GetDirectoryName(fileName);
        if (directory == null)
            throw new InvalidDataException("File metadata.json not found");
        var metadataFileName = Path.Combine(directory, "metadata.json");
        var text = await File.ReadAllTextAsync(metadataFileName, cancellationToken);
        var metadata = JsonConvert.DeserializeObject<DocumentMetadata>(text);
        if (metadata == null)
            throw new InvalidDataException("File metadata.json malformed!");

        return metadata;
    }

    private string[] GetFilesToProcess(string inputDirectory)
    {
        Directory.CreateDirectory(InputPath);
        return Directory.GetFiles(inputDirectory, "*.md", SearchOption.AllDirectories)
            .Where(f => Path.GetFileName(Path.GetDirectoryName(f)) != IgnoreDir)
            .ToArray();
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
