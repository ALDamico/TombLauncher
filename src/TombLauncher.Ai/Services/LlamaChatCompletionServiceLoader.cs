using LLama;
using LLama.Common;
using LLamaSharp.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.ChatCompletion;
using TombLauncher.Ai.Abstractions;

namespace TombLauncher.Ai.Services;

public class LlamaChatCompletionServiceLoader : IChatCompletionServiceLoader
{
    private readonly IWeightsLoader _weightsLoader;
    private readonly ModelParams _modelParams;

    public LlamaChatCompletionServiceLoader(IWeightsLoader weightsLoader, ModelParams modelParams)
    {
        _weightsLoader = weightsLoader;
        _modelParams = modelParams;
    }
    public async Task<IChatCompletionService> LoadChatCompletionService(IProgress<float> progress,
        CancellationToken cancellationToken)
    {
        var weights = await _weightsLoader.LoadWeightsAsync(_modelParams, progress, cancellationToken);
        var llamaContext = new LLamaContext(weights, _modelParams);
        var executor = new InteractiveExecutor(llamaContext);
        IsLoaded = true;
        return new LLamaSharpChatCompletion(executor);
    }

    public bool IsLoaded { get; private set; }
}