using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using TombLauncher.Ai.Abstractions;
using TombLauncher.Core.PlatformSpecific;
using TombLauncher.Gamepad.SupportMatrix;

namespace TombLauncher.Ai.Services;

public class RagServiceLoader : ITroubleshootingServiceLoader
{
    private readonly IChatCompletionServiceLoader _chatCompletionServiceLoader;
    private readonly VectorSearchService _vectorSearchService;
    private readonly Kernel _kernel;
    private readonly PromptExecutionSettings _promptExecutionSettings;
    private readonly ILogger<RagService> _ragServiceLogger;
    private readonly IPlatformSpecificFeatures _platformSpecificFeatures;
    private readonly GamepadSupportMatrix _gamepadSupportMatrix;

    public RagServiceLoader(IChatCompletionServiceLoader chatCompletionServiceLoader,
        VectorSearchService vectorSearchService,
        Kernel kernel,
        PromptExecutionSettings promptExecutionSettings,
        ILogger<RagService> ragServiceLogger, 
        IPlatformSpecificFeatures platformSpecificFeatures,
        GamepadSupportMatrix gamepadSupportMatrix)
    {
        _chatCompletionServiceLoader = chatCompletionServiceLoader;
        _vectorSearchService = vectorSearchService;
        _kernel = kernel;
        _promptExecutionSettings = promptExecutionSettings;
        _ragServiceLogger = ragServiceLogger;
        _platformSpecificFeatures = platformSpecificFeatures;
        _gamepadSupportMatrix = gamepadSupportMatrix;
    }

    public async Task<ITroubleshootingService> Load(IProgress<float> progress, CancellationToken cancellationToken)
    {
        var chatCompletionService =
            await _chatCompletionServiceLoader.LoadChatCompletionService(progress, cancellationToken);

        return new RagService(_kernel, _vectorSearchService, chatCompletionService, _promptExecutionSettings, _ragServiceLogger, _platformSpecificFeatures, _gamepadSupportMatrix);
    }
}