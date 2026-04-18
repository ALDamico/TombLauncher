using TombLauncher.Ai.Abstractions;

namespace TombLauncher.Ai.Services;

public class RagServiceLoader : ITroubleshootingServiceLoader
{
    private readonly IChatCompletionServiceLoader _chatCompletionServiceLoader;
    private readonly VectorSearchService _vectorSearchService;

    public RagServiceLoader(IChatCompletionServiceLoader chatCompletionServiceLoader, VectorSearchService vectorSearchService)
    {
        _chatCompletionServiceLoader = chatCompletionServiceLoader;
        _vectorSearchService = vectorSearchService;
    }

    public async Task<ITroubleshootingService> Load(IProgress<float> progress, CancellationToken cancellationToken)
    {
        var chatCompletionService =
            await _chatCompletionServiceLoader.LoadChatCompletionService(progress, cancellationToken);

        return new RagService(_vectorSearchService, chatCompletionService);
    }
}