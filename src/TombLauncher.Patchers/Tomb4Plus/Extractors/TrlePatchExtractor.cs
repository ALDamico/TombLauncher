using Microsoft.Extensions.Logging;
using TombLauncher.Contracts.Extensions;
using TombLauncher.Patchers.Tomb4Plus.Enums;
using TombLauncher.Patchers.Tomb4Plus.Extensions;
using TombLauncher.Patchers.Tomb4Plus.Models;

namespace TombLauncher.Patchers.Tomb4Plus.Extractors;

public class TrlePatchExtractor
{
    private readonly ILogger<TrlePatchExtractor> _logger;

    public TrlePatchExtractor(ILogger<TrlePatchExtractor> logger)
    {
        _logger = logger;
    }
    
    private AudioInfo? ReadAudioInfo(BinaryReader reader, bool isUsingRemappedMemory, PatchBinaryType patchType)
    {
        if (patchType != PatchBinaryType.TrepExe)
            return null;
        var audioInfo = new AudioInfo();

        if (isUsingRemappedMemory)
        {
            var usingBass = reader.IsNopAtRange(0x000F66C0, 0x000F6E7E);
            if (usingBass)
            {
                audioInfo.NewAudioSystem = true;
                audioInfo.OldCdTriggerSystem = false;
            }
        }

        audioInfo.DisableLaraHitSfx =
            reader.CompareDataAtAddress(0x0000B02A, [0x90, 0x90, 0x90, 0x90, 0x90]).NullIf(false);

        const sbyte defaultLaraHitSfx = 50;

        audioInfo.LaraHitSfx = reader.ReadSByteAt(0x0000B029).NullIf(defaultLaraHitSfx);

        audioInfo.DisableNoAmmoSfx =
            reader.CompareDataAtAddress(0x0002D814, [0x90, 0x90, 0x90, 0x90, 0x90]).NullIf(false);

        const sbyte defaultNoAmmoSfx = 48;
        audioInfo.NoAmmoSfx = reader.ReadSByteAt(0x0002D813).NullIf(defaultNoAmmoSfx);

        const byte defaultJeepTrack = 98;
        audioInfo.InsideJeepTrack = reader.ReadByteAt(0x000663FE).NullIf(defaultJeepTrack);

        const byte defaultOutsideJeepTrack = 110;
        audioInfo.OutsideJeepTrack = reader.ReadByteAt(0x00066EAD).NullIf(defaultOutsideJeepTrack);

        const byte defaultSecretTrack = 5;
        audioInfo.SecretTrack = reader.ReadByteAt(0x0004AACC).NullIf(defaultSecretTrack);

        var changeLoopedAudioTrackRange = reader.IsNopAtRange(0x0004BE2C, 0x0004BE3C);
        if (changeLoopedAudioTrackRange)
        {
            const byte defaultLoopedAudioTrack = 105;
            audioInfo.FirstLoopedAudioTrack = reader.ReadByteAt(0x0004BE28).NullIf(defaultLoopedAudioTrack);
        }

        return audioInfo;
    }

    private BarsInfo? ReadBarsInfo(BinaryReader reader, PatchBinaryType patchType)
    {
        if (patchType != PatchBinaryType.TrepExe)
            return null;
        var barsInfo = new BarsInfo();

        var barTypeByte = reader.ReadByteAt(0x0007b0f9);
        var gradientType = barTypeByte switch
        {
            Constants.Tr5BarType => GradientType.GradientTr5,
            Constants.FlatBarType => GradientType.GradientFlat,
            _ => GradientType.Normal
        };

        barsInfo.HealthBar = ReadHealthBarInfo(reader, gradientType);
        barsInfo.PoisonBar = ReadPoisonBarInfo(reader, gradientType);
        barsInfo.AirBar = ReadAirBarInfo(reader, gradientType);
        barsInfo.SprintBar = ReadSprintBarInfo(reader, gradientType);
        barsInfo.LoadingBar = ReadLoadingBarInfo(reader, gradientType);

        return barsInfo;
    }

    private BarStyle? ReadLoadingBarInfo(BinaryReader reader, GradientType gradientType)
    {
        throw new NotImplementedException();
    }

    private BarStyle? ReadSprintBarInfo(BinaryReader reader, GradientType gradientType)
    {
        throw new NotImplementedException();
    }

    private BarStyle? ReadAirBarInfo(BinaryReader reader, GradientType gradientType)
    {
        throw new NotImplementedException();
    }

    private BarStyle? ReadPoisonBarInfo(BinaryReader reader, GradientType gradientType)
    {
        throw new NotImplementedException();
    }

    private BarStyle? ReadHealthBarInfo(BinaryReader reader, GradientType gradientType)
    {
        var healthBarMainColor = reader.GetBgrColorAtAddress(0x0007B5B0);
        var healthBarFadeColor = reader.GetBgrColorAtAddress(0x0007B5BA);
        var healthBarAlternativeColor = reader.GetBgrColorAtAddress(0x0007B5AB);

        var healthBarInfo = new BarStyle();

        if (healthBarMainColor.R != 255 || healthBarMainColor.G != 0 || healthBarMainColor.B != 0 ||
            healthBarFadeColor.R != 0 || healthBarFadeColor.G != 0 || healthBarFadeColor.B != 0 ||
            healthBarAlternativeColor.R != 0 || healthBarAlternativeColor.G != 255 ||
            healthBarAlternativeColor.B != 0 || gradientType != GradientType.Normal)
            ConstructBar(healthBarInfo, healthBarMainColor, healthBarFadeColor, gradientType);

        UpdateBarBackgroundColors(reader, healthBarInfo);

        const short defaultHealthBarWidth = 150;
        healthBarInfo.Width = reader.ReadShortAt(0x0007B5C5).NullIf(defaultHealthBarWidth);

        const byte defaultHealthBarHeight = 12;
        healthBarInfo.Height = reader.ReadByteAt(0x0007B5C3).NullIf(defaultHealthBarHeight);

        healthBarInfo.IsAnimated = reader.CompareDataAtAddress(0x0007B5CC, [0x50, 0xD7]).NullIf(false);
        
        if (healthBarInfo.HasAnyValue())
            return healthBarInfo;

        return null;
    }

    private void UpdateBarBackgroundColors(BinaryReader reader, BarStyle bar)
    {
        var address1 = reader.ReadByteAt(0x00079083);
        var address2 = reader.ReadByteAt(0x0007B316);

        ColorRgb? border1Color = null;
        ColorRgb? border2Color = null;

        if (address1 != 0x83)
            border1Color = reader.GetBgrColorAtAddress(0x00079084);

        if (address2 != 0x83)
            border2Color = reader.GetBgrColorAtAddress(0x0007B317);

        if (border1Color != border2Color)
        {
            _logger.LogWarning("Border color: MISMATCH");
            return;
        }

        if (border1Color == null)
            return;

        bar.BorderRect = new BarRect()
        {
            UpperLeftColor = border1Color,
            UpperRightColor = border1Color,
            LowerLeftColor = border1Color,
            LowerRightColor = border1Color
        };
    }

    private void ConstructBar(BarStyle bar, ColorRgb mainColor, ColorRgb fadeColor, GradientType gradientType)
    {
        switch (gradientType)
        {
            case GradientType.Normal:
                bar.UpperRect = new BarRect()
                {
                    UpperLeftColor = fadeColor,
                    UpperRightColor = fadeColor,
                    LowerRightColor = mainColor,
                    LowerLeftColor = mainColor
                };

                bar.LowerRect = new BarRect()
                {
                    UpperLeftColor = mainColor,
                    UpperRightColor = mainColor,
                    LowerRightColor = fadeColor,
                    LowerLeftColor = fadeColor
                };

                break;
            
            case GradientType.GradientTr5:
                bar.UpperRect = new BarRect()
                {
                    UpperLeftColor = Constants.BlackColor,
                    UpperRightColor = Constants.BlackColor,
                    LowerRightColor = fadeColor,
                    LowerLeftColor = mainColor
                };

                bar.LowerRect = new BarRect()
                {
                    UpperLeftColor = mainColor,
                    UpperRightColor = fadeColor,
                    LowerRightColor = Constants.BlackColor,
                    LowerLeftColor = Constants.BlackColor
                };

                break;
            case GradientType.GradientFlat:
                bar.UpperRect = new BarRect()
                {
                    UpperLeftColor = mainColor,
                    UpperRightColor = fadeColor,
                    LowerRightColor = fadeColor,
                    LowerLeftColor = mainColor
                };

                bar.LowerRect = new BarRect()
                {
                    UpperLeftColor = mainColor,
                    UpperRightColor = fadeColor,
                    LowerRightColor = fadeColor,
                    LowerLeftColor = mainColor
                };
                break;
        }
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
        return output;
    }
}