using LLama;
using LLama.Common;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TombLauncher.Ai.Configuration;
using TombLauncher.Core.PlatformSpecific;

namespace TombLauncher.Ai.Factories;

public static class EmbedderFactory
{
    public static LLamaEmbedder GetEmbedder(IServiceProvider sp)
    {
        var options = sp.GetRequiredService<IAiConfig>();
        var platformSpecificFeatures = sp.GetRequiredService<IPlatformSpecificFeatures>();
        var modelPath = Path.Combine(platformSpecificFeatures.GetAppDataDirectory(), options.ModelsPath, options.EmbeddingModelFileName);
        var modelParameters = new ModelParams(modelPath)
        {
            Embeddings = true,
            ContextSize = options.EmbeddingContextLength
        };
        var weights = LLamaWeights.LoadFromFile(modelParameters);

        return new LLamaEmbedder(weights, modelParameters, sp.GetRequiredService<ILogger<LLamaEmbedder>>());
    }
}