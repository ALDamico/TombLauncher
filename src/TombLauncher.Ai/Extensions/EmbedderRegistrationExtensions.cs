using System.ClientModel;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using OpenAI;
using TombLauncher.Ai.Abstractions;
using TombLauncher.Ai.Configuration;
using TombLauncher.Ai.Factories;
using TombLauncher.Ai.Mappers;
using TombLauncher.Ai.Plugins;
using TombLauncher.Ai.Services;
using TombLauncher.Ai.Services.AiBackends;
using TombLauncher.Ai.Utils;
using TombLauncher.Contracts.Enums;

namespace TombLauncher.Ai.Extensions;

public static class EmbedderRegistrationExtensions
{
    public static IServiceCollection RegisterKnowledgeBaseEmbedder(this IServiceCollection serviceCollection)
    {
        return serviceCollection
            .AddSingleton<OpenAIClient>(sp =>
            {
                var aiConfig = sp.GetRequiredService<IOptions<AiConfig>>().Value;
                return new OpenAIClient(new ApiKeyCredential(aiConfig.ApiKey ?? "ollama"),
                    new OpenAIClientOptions() { Endpoint = new Uri(OllamaEndpointHelper.NormalizeEndpoint(aiConfig.Endpoint ?? "http://localhost:11434")) });
            })
            .AddSingleton<IEmbeddingGenerator<string, Embedding<float>>>(sp =>
            {
                var config = sp.GetRequiredService<IOptions<AiConfig>>().Value;
                var builder = Kernel.CreateBuilder();
                builder.Services.AddOpenAIEmbeddingGenerator(
                    config.EmbeddingModelId!,
                    sp.GetRequiredService<OpenAIClient>());
                return builder.Build().Services.GetRequiredService<IEmbeddingGenerator<string, Embedding<float>>>();
            })
            .AddHostedService<EmbedderService>()
            .AddSingleton<KnowledgeBaseWriter>();
    }

    public static IServiceCollection RegisterAiFeatures(this IServiceCollection serviceCollection)
    {
        return serviceCollection
            .AddScoped<ITroubleshootingServiceLoader, RagServiceLoader>()
            .AddSingleton<VectorSearchService>()
            .AddSingleton<IEmbeddingGenerator<string, Embedding<float>>>(sp =>
                sp.GetRequiredService<Kernel>().Services
                    .GetRequiredService<IEmbeddingGenerator<string, Embedding<float>>>())
            .AddScoped<TroubleshootingContextService>()
            .AddScoped<IChatCompletionServiceLoader, OpenAiCompatibleChatCompletionServiceLoader>()
            .AddScoped<PromptExecutionSettings>(_ => new PromptExecutionSettings()
                { FunctionChoiceBehavior = FunctionChoiceBehavior.Auto(), ExtensionData = new Dictionary<string, object> { ["temperature"] = 0.65 }})
            .AddSingleton<OpenAIClient>(sp =>
            {
                var aiConfig = sp.GetRequiredService<IAiConfig>();
                return new OpenAIClient(new ApiKeyCredential(aiConfig.ApiKey ?? "ollama"),
                    new OpenAIClientOptions() { Endpoint = new Uri(OllamaEndpointHelper.NormalizeEndpoint(aiConfig.Endpoint ?? "http://localhost:11434")) });
            })
            .AddSingleton<Kernel>(sp =>
            {
                var config = sp.GetRequiredService<IAiConfig>();
                var kernelBuilder = Kernel.CreateBuilder();
                try
                {
                    kernelBuilder.Services.AddOpenAIEmbeddingGenerator(
                        config.EmbeddingModelId!,
                        sp.GetRequiredService<OpenAIClient>());
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
                return kernelBuilder.Build();
            })
            .AddHttpClient()
            .AddSingleton<ModelMapper>()
            .AddKeyedSingleton<IAiBackendService, OllamaBackendService>(AiBackendType.Ollama)
            .AddKeyedSingleton<IAiBackendService, LmStudioBackendService>(AiBackendType.LmStudio)
            .AddSingleton<AiBackendFactory>();
    }
}
