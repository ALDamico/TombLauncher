using LLama;

namespace TombLauncher.Ai.Models;

public class WeightsData
{
    public required LLamaWeights Weights { get; init; }
    public int EffectiveGpuLayerCount { get; init; }
}