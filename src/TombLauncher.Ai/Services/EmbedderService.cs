using LLama;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TombLauncher.Ai.Models;

namespace TombLauncher.Ai.Services;

public class EmbedderService : IHostedService
{
    private readonly IHostApplicationLifetime _lifetime;
    private readonly ILogger<EmbedderService> _logger;
    private readonly LLamaEmbedder _embedder;
    private readonly KnowledgeBaseWriter _knowledgeBaseWriter;
    private const string InputPath = "./Input";

    public EmbedderService(IHostApplicationLifetime lifetime, ILogger<EmbedderService> logger, LLamaEmbedder embedder, KnowledgeBaseWriter knowledgeBaseWriter)
    {
        _lifetime = lifetime;
        _logger = logger;
        _embedder = embedder;
        _knowledgeBaseWriter = knowledgeBaseWriter;
    }
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var files = GetFilesToProcess(InputPath);
        var allChunks = new List<Chunk>();

        foreach (var file in files)
        {
            var fileContent = await File.ReadAllTextAsync(file, cancellationToken);
            _logger.LogInformation("Computing embedding embedding for {File}", file);
            var embeddings = await _embedder.GetEmbeddings(fileContent, cancellationToken);
            var chunks = embeddings.Select(e => new Chunk()
            {
                ChunkText = fileContent, DocumentTitle = Path.GetFileName(file), SectionTitle = Path.GetFileName(file),
                Embedding = e
            });
            allChunks.AddRange(chunks);
        }
        _logger.LogInformation("Saving embeddings");
        await _knowledgeBaseWriter.WriteChunks(allChunks, cancellationToken);
        
        _lifetime.StopApplication();
    }

    private string[] GetFilesToProcess(string inputDirectory)
    {
        Directory.CreateDirectory(InputPath);
        return Directory.GetFiles(inputDirectory, "*.md");
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}