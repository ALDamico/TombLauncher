using Microsoft.SemanticKernel.ChatCompletion;

namespace TombLauncher.Ai.Abstractions;

public interface IChatCompletionServiceLoader
{
    Task<IChatCompletionService> LoadChatCompletionService(IProgress<float> progress,
        CancellationToken cancellationToken);
    bool IsLoaded { get; }
}