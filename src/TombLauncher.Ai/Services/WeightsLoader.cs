using System.Globalization;
using LLama;
using LLama.Common;
using TombLauncher.Ai.Abstractions;
using TombLauncher.Ai.Configuration;
using TombLauncher.Ai.Models;
using TombLauncher.Ai.Utils;
using TombLauncher.Core.Extensions;

namespace TombLauncher.Ai.Services;

public class WeightsLoader : IWeightsLoader
{
    private readonly double _offloadPercentage;

    public WeightsLoader(IAiConfig aiConfig)
    {
        _offloadPercentage = aiConfig.GpuOffloadPercentage.GetValueOrDefault();
    }
    public async Task<WeightsData> LoadWeightsAsync(ModelParams modelParams, IProgress<float> progressReporter, CancellationToken cancellationToken)
    {
        var weights = await LLamaWeights.LoadFromFileAsync(modelParams, cancellationToken, progressReporter);
        var gpuOverlays = weights.Metadata.FirstOrDefault(k => k.Key.EndsWith("block_count"));
        var overlayCount = int.MaxValue;

        if (gpuOverlays.Key != null && gpuOverlays.Value.IsNotNullOrWhiteSpace())
            int.TryParse(gpuOverlays.Value, NumberStyles.Integer, CultureInfo.InvariantCulture, out overlayCount);

        overlayCount = AiConfigUtils.ComputeGpuLayerCount(overlayCount, _offloadPercentage);

        modelParams.GpuLayerCount = overlayCount;
        return new WeightsData()
        {
            Weights = weights,
            EffectiveGpuLayerCount = overlayCount
        };
    }
}