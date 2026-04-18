using LLama;
using LLama.Common;
using TombLauncher.Ai.Abstractions;

namespace TombLauncher.Ai.Services;

public class WeightsLoader : IWeightsLoader
{
    public async Task<LLamaWeights> LoadWeightsAsync(ModelParams modelParams, IProgress<float> progressReporter, CancellationToken cancellationToken)
    {
        var weights = await LLamaWeights.LoadFromFileAsync(modelParams, cancellationToken, progressReporter);
        return weights;
    }
}