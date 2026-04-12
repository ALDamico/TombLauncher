using LLama;
using LLama.Common;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TombLauncher.Ai.Configuration;

namespace TombLauncher.Ai.Factories;

public static class EmbedderFactory
{
    public static LLamaEmbedder GetEmbedder(IServiceProvider sp)
    {
        var options = sp.GetRequiredService<IOptions<KnowledgeBaseEmbedderConfiguration>>().Value;
        var modelPath = Path.Combine(options.ModelsPath, options.ModelFileName);
        var modelParameters = new ModelParams(modelPath)
        {
            Embeddings = true,
            ContextSize = options.ContextLength
        };
        var weights = LLamaWeights.LoadFromFile(modelParameters);

        return new LLamaEmbedder(weights, modelParameters, sp.GetRequiredService<ILogger<LLamaEmbedder>>());
    }
}