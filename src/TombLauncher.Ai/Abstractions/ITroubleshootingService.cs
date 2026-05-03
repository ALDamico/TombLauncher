using Microsoft.SemanticKernel.ChatCompletion;
using TombLauncher.Ai.Models;
using TombLauncher.Contracts.Progress;

namespace TombLauncher.Ai.Abstractions;

public interface ITroubleshootingService : IDisposable
{
    IAsyncEnumerable<string> AskAsync(IProgress<DownloadProgressInfo> progress, string query, TroubleshootingContext troubleshootingContext, ChatHistory chatHistory,
        CancellationToken cancellationToken);
}