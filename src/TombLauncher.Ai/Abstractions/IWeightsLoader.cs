using LLama.Common;
using TombLauncher.Ai.Models;

namespace TombLauncher.Ai.Abstractions;

public interface IWeightsLoader
{
    Task<WeightsData> LoadWeightsAsync(ModelParams modelParams, IProgress<float> progressReporter, CancellationToken cancellationToken);
}