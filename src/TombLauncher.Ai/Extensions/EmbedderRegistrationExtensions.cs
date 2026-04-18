using LLama;
using LLama.Common;
using Microsoft.Extensions.DependencyInjection;
using TombLauncher.Ai.Abstractions;
using TombLauncher.Ai.Configuration;
using TombLauncher.Ai.Factories;
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
                        ContextSize = 8192,
                        GpuLayerCount = aiConfig.GpuLayerCount.GetValueOrDefault()
                    };
            })
            .AddScoped<IWeightsLoader, WeightsLoader>()
            .AddScoped<IChatCompletionServiceLoader, LlamaChatCompletionServiceLoader>();
    }
}