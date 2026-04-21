using LLama.Common;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using TombLauncher.Ai.Abstractions;
using TombLauncher.Ai.Configuration;
using TombLauncher.Ai.Factories;
using TombLauncher.Ai.Plugins;
using TombLauncher.Ai.Services;
using TombLauncher.Core.PlatformSpecific;

namespace TombLauncher.Ai.Extensions;

public static class EmbedderRegistrationExtensions
{
    public static IServiceCollection RegisterKnowledgeBaseEmbedder(this IServiceCollection serviceCollection)
    {
        return serviceCollection.AddHostedService<EmbedderService>()
            .AddSingleton(EmbedderFactory.GetEmbedder)
            .AddSingleton<KnowledgeBaseWriter>();
    }

    public static IServiceCollection RegisterAiFeatures(this IServiceCollection serviceCollection)
    {
        return serviceCollection.AddScoped<ITroubleshootingServiceLoader, RagServiceLoader>()
            .AddSingleton<VectorSearchService>()
            .AddSingleton(EmbedderFactory.GetEmbedder)
            .AddScoped<ModelParams>(sp =>
            {
                var platformSpecificFeatures = sp.GetRequiredService<IPlatformSpecificFeatures>();
                var aiConfig = sp.GetRequiredService<IAiConfig>();
                var modelId = aiConfig.ModelName!;
                var modelsDirectory = Path.Combine(platformSpecificFeatures.GetAppDataDirectory(), "Models");
                return new ModelParams(Path.Combine(modelsDirectory, modelId))
                {
                    ContextSize = 8192
                };
            })
            .AddScoped<IWeightsLoader, WeightsLoader>()
            .AddScoped<TroubleshootingContextService>()
            .AddScoped<IChatCompletionServiceLoader, LlamaChatCompletionServiceLoader>()
            .AddScoped<PromptExecutionSettings>(_ => new PromptExecutionSettings()
                { FunctionChoiceBehavior = FunctionChoiceBehavior.Auto() })
            .AddSingleton<GameDiagnosticsPlugin>()
            .AddSingleton<AiModelRegistry>()
            .AddSingleton<Kernel>(sp =>
            {
                var kernel = Kernel.CreateBuilder().Build();
                var gameDiagnosticsPlugin = sp.GetRequiredService<GameDiagnosticsPlugin>();
                kernel.Plugins.AddFromObject(gameDiagnosticsPlugin);
                return kernel;
            })
            .AddSingleton<ModelDownloadService>();
    }
}