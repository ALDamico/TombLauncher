using Microsoft.SemanticKernel.ChatCompletion;

namespace TombLauncher.Ai.Abstractions;

public interface ITroubleshootingService : IDisposable
{
    IAsyncEnumerable<string> AskAsync(string query, ChatHistory chatHistory,
        CancellationToken cancellationToken);
}