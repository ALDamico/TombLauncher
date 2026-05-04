using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using TombLauncher.Ai.Abstractions;
using TombLauncher.Ai.Configuration;

namespace TombLauncher.Ai.Services;

public class OpenAiCompatibleChatCompletionServiceLoader : IChatCompletionServiceLoader
{
    private readonly IAiConfig _aiConfig;

    public OpenAiCompatibleChatCompletionServiceLoader(IAiConfig aiConfig)
    {
        _aiConfig = aiConfig;
    }
    public async Task<IChatCompletionService> LoadChatCompletionService(IProgress<float> progress,
        CancellationToken cancellationToken)
    {
        return new OpenAIChatCompletionService(_aiConfig.ModelName!, new Uri(_aiConfig.Endpoint), _aiConfig.ApiKey ?? "ollama");
    }

    public bool IsLoaded { get; private set; }
}