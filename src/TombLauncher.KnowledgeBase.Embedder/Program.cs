using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TombLauncher.Ai.Configuration;
using TombLauncher.Ai.Extensions;
using TombLauncher.Contracts.Progress;
using TombLauncher.Core.Extensions;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration(builder => builder.AddJsonFile("appsettings.json"))
    .ConfigureServices((ctx, services) =>
    {
        services.Configure<AiConfig>(ctx.Configuration.GetSection("Embedder"));
        services.RegisterKnowledgeBaseEmbedder()
            .AddEmbedderLogging();
    })
    .Build();

var cts = new CancellationTokenSource();
Console.CancelKeyPress += (_, e) => { e.Cancel = true; cts.Cancel(); };

var cancellationToken = cts.Token;
var options = host.Services.GetRequiredService<IOptions<AiConfig>>().Value;
var logger = host.Services.GetRequiredService<ILogger<Program>>();
var modelPath = Path.Combine(options.ModelsPath, options.EmbeddingModelFileName);
Directory.CreateDirectory(options.ModelsPath);
if (!File.Exists(modelPath))
{
    await DownloadModel(options.EmbeddingModelUrl, modelPath, logger, cancellationToken);
}
    
await host.RunAsync();
return;

async Task DownloadModel(string modelUrl, string filePath, ILogger<Program> downloadLogger, CancellationToken ct)
{
    ct.ThrowIfCancellationRequested();
    using var httpClient = new HttpClient();
    await using var file = new FileStream(filePath, FileMode.Create);
    await httpClient.DownloadAsync(modelUrl, file,
        new Progress<DownloadProgressInfo>(dli => downloadLogger.LogInformation("Downloading from {Url}: {Percentage}%",
            modelUrl, (double)dli.BytesDownloaded / dli.TotalBytes * 100)), ct);
}