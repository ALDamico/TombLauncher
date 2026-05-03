using TombLauncher.Ai.Services;

namespace TombLauncher.Ai.Utils;

public static class AiConfigUtils
{
    public static int ComputeGpuLayerCount(int maxLayers, double offloadPercentage)
    {
        if (offloadPercentage > 1)
            offloadPercentage = 1;
        return (int)(maxLayers * offloadPercentage);
    }
    
    public static string LoadSystemPrompt()
    {
        var asm = typeof(RagService).Assembly;
        using var stream = asm.GetManifestResourceStream("TombLauncher.Ai.Agents.SystemPrompt.md")!;
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
}