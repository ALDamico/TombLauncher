using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TombLauncher.Ai.Configuration;
using TombLauncher.Ai.Extensions;
using TombLauncher.Core.Extensions;
using TombLauncher.KnowledgeBase.Embedder.Configuration;
using TombLauncher.KnowledgeBase.Embedder.Services;

var shouldUpload = args.Contains("--upload");

var host = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration(builder =>
    {
        builder.AddJsonFile("appsettings.json");
        builder.AddJsonFile("appsettings.local.json", optional: true);
    })
    .ConfigureServices((ctx, services) =>
    {
        services.Configure<AiConfig>(ctx.Configuration.GetSection("Embedder"));
        services.RegisterKnowledgeBaseEmbedder()
            .AddEmbedderLogging();

        if (shouldUpload)
        {
            services.Configure<SftpConfig>(ctx.Configuration.GetSection("Sftp"));
            services.AddSingleton<KbUploadService>();
        }
    })
    .Build();

var cts = new CancellationTokenSource();
Console.CancelKeyPress += (_, e) => { e.Cancel = true; cts.Cancel(); };

var cancellationToken = cts.Token;

var uploader = shouldUpload ? host.Services.GetRequiredService<KbUploadService>() : null;

await host.RunAsync(cancellationToken);

if (uploader != null)
    await uploader.UploadAsync(cancellationToken);
