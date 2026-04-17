using Microsoft.SemanticKernel.ChatCompletion;

namespace TombLauncher.Ai.Abstractions;

public interface ITroubleshootingService
{
    IAsyncEnumerable<string> AskAsync(string query, ChatHistory chatHistory,
        CancellationToken cancellationToken);
}