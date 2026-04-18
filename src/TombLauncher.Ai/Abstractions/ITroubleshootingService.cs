using Microsoft.SemanticKernel.ChatCompletion;
using TombLauncher.Ai.Models;

namespace TombLauncher.Ai.Abstractions;

public interface ITroubleshootingService : IDisposable
{
    IAsyncEnumerable<string> AskAsync(string query, TroubleshootingContext troubleshootingContext, ChatHistory chatHistory,
        CancellationToken cancellationToken);
}