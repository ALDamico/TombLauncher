using Microsoft.Extensions.Logging;
using TombLauncher.Patchers.Tomb4Plus.Enums;
using TombLauncher.Patchers.Tomb4Plus.Extensions;
using TombLauncher.Patchers.Tomb4Plus.Models;

namespace TombLauncher.Patchers.Tomb4Plus.Extractors;

public partial class TrlePatchExtractor
{
    private readonly ILogger<TrlePatchExtractor> _logger;

    public TrlePatchExtractor(ILogger<TrlePatchExtractor> logger)
    {
        _logger = logger;
    }

    public TrlePatchExecutorOutput ReadBinaryFile(string exeFilePath, bool isExtendedFileSize,
        bool isUsingRemappedMemory, PatchBinaryType patchType)
    {
        var output = new TrlePatchExecutorOutput()
        {
            GlobalInfo = new GlobalInfo(),
            MetaInfo = new MetaInfo(),
            GlobalLevelInfo = new GlobalLevelInfo()
        };

        using var binaryReader = new BinaryReader(File.OpenRead(exeFilePath));
        output.GlobalLevelInfo.AudioInfo = ReadAudioInfo(binaryReader, isUsingRemappedMemory, patchType);
        output.GlobalLevelInfo.BarsInfo = ReadBarsInfo(binaryReader, patchType);
        output.GlobalLevelInfo.FontInfo = ReadFontInfo(binaryReader);
        output.GlobalLevelInfo.GfxInfo = ReadGfxInfo(binaryReader, patchType);
        output.GlobalLevelInfo.ObjectsInfo = ReadObjectsInfo(binaryReader, patchType);
        output.GlobalLevelInfo.EnvironmentInfo = ReadEnvironmentInfo(binaryReader, patchType);
        output.GlobalLevelInfo.LaraInfo = ReadLaraInfo(binaryReader, patchType);
        output.GlobalLevelInfo.StatInfo = ReadStatInfo(binaryReader, patchType);
        return output;
    }
}
