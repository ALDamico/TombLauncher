using TombLauncher.Contracts.Ai;

namespace TombLauncher.Ai.Utils;

public static class ChunkUtils
{
    private const int ChunkSizeChars = AiConstants.MaxChunkLength * AiConstants.CharsPerChunk;
    private const int StepChars = (AiConstants.MaxChunkLength - AiConstants.Overlap) * AiConstants.CharsPerChunk;

    public static List<string> SplitChunks(string text)
    {
        var result = new List<string>();
        var index = 0;
        while (index < text.Length)
        {
            var length = Math.Min(ChunkSizeChars, text.Length - index);
            result.Add(text.Substring(index, length));
            index += StepChars;
        }

        return result;
    }
}