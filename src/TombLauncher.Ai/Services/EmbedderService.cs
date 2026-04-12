using LLama;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

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

        foreach (var file in files)
        {
            var fileContent = await File.ReadAllTextAsync(file, cancellationToken);
            var embeddings = await _embedder.GetEmbeddings(fileContent, cancellationToken);
            _logger.LogInformation(string.Join(',', embeddings[0]));
        }
        
        _lifetime.StopApplication();
    }

    private string[] GetFilesToProcess(string inputDirectory)
    {
        Directory.CreateDirectory(InputPath);
        return Directory.GetFiles(inputDirectory, "*.md");
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}