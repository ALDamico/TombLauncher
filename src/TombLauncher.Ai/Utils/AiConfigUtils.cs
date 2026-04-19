namespace TombLauncher.Ai.Utils;

public static class AiConfigUtils
{
    public static int ComputeGpuLayerCount(int maxLayers, double offloadPercentage)
    {
        if (offloadPercentage > 1)
            offloadPercentage = 1;
        return (int)(maxLayers * offloadPercentage);
    }
}