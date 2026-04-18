using LLama;
using LLama.Common;

namespace TombLauncher.Ai.Abstractions;

public interface IWeightsLoader
{
    Task<LLamaWeights> LoadWeightsAsync(ModelParams modelParams, IProgress<float> progressReporter, CancellationToken cancellationToken);
}