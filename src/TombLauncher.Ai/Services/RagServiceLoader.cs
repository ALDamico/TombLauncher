using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using TombLauncher.Ai.Abstractions;
using TombLauncher.Ai.Plugins;

namespace TombLauncher.Ai.Services;

public class RagServiceLoader : ITroubleshootingServiceLoader
{
    private readonly IChatCompletionServiceLoader _chatCompletionServiceLoader;
    private readonly VectorSearchService _vectorSearchService;
    private readonly Kernel _kernel;
    private readonly GameDiagnosticsPlugin _gameDiagnosticsPlugin;
    private readonly PromptExecutionSettings _promptExecutionSettings;
    private readonly ILogger<RagService> _ragServiceLogger;

    public RagServiceLoader(IChatCompletionServiceLoader chatCompletionServiceLoader,
        VectorSearchService vectorSearchService,
        Kernel kernel,
        GameDiagnosticsPlugin gameDiagnosticsPlugin,
        PromptExecutionSettings promptExecutionSettings,
        ILogger<RagService> ragServiceLogger)
    {
        _chatCompletionServiceLoader = chatCompletionServiceLoader;
        _vectorSearchService = vectorSearchService;
        _kernel = kernel;
        _gameDiagnosticsPlugin = gameDiagnosticsPlugin;
        _promptExecutionSettings = promptExecutionSettings;
        _ragServiceLogger = ragServiceLogger;
    }

    public async Task<ITroubleshootingService> Load(IProgress<float> progress, CancellationToken cancellationToken)
    {
        var chatCompletionService =
            await _chatCompletionServiceLoader.LoadChatCompletionService(progress, cancellationToken);

        return new RagService(_kernel, _vectorSearchService, chatCompletionService, _gameDiagnosticsPlugin, _promptExecutionSettings, _ragServiceLogger);
    }
}