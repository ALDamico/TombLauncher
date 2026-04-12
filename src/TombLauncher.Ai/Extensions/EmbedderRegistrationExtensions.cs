using Microsoft.Extensions.DependencyInjection;
using TombLauncher.Ai.Factories;
using TombLauncher.Ai.Services;

namespace TombLauncher.Ai.Extensions;

public static class EmbedderRegistrationExtensions
{
    public static IServiceCollection RegisterKnowledgeBaseEmbedder(this IServiceCollection serviceCollection)
    {
        return serviceCollection.AddHostedService<EmbedderService>()
            .AddSingleton(EmbedderFactory.GetEmbedder);
    }
}