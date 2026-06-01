using Microsoft.Extensions.Logging;
using TombLauncher.Contracts.Extensions;
using TombLauncher.Patchers.Tomb4Plus.Enums;
using TombLauncher.Patchers.Tomb4Plus.Extensions;
using TombLauncher.Patchers.Tomb4Plus.Models;

namespace TombLauncher.Patchers.Tomb4Plus.Extractors;

public partial class TrlePatchExtractor
{
    private const short DefaultBarWidth = 150;
    private const byte DefaultBarHeight = 12;
    private const short DefaultAirBarOffset = 490;

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

        healthBarInfo.Width = reader.ReadShortAt(0x0007B5C5).NullIf(DefaultBarWidth);
        healthBarInfo.Height = reader.ReadByteAt(0x0007B5C3).NullIf(DefaultBarHeight);
        healthBarInfo.IsAnimated = reader.CompareDataAtAddress(0x0007B5CC, [0x50, 0xD7]).NullIf(false);

        if (healthBarInfo.HasAnyValue())
            return healthBarInfo;

        return null;
    }

    private BarStyle? ReadPoisonBarInfo(BinaryReader reader, GradientType gradientType)
    {
        var poisonBarInfo = new BarStyle();
        var poisonBarMainColor = reader.GetBgrColorAtAddress(0x0007B5B0);
        var poisonBarFadeColor = reader.GetBgrColorAtAddress(0x0007B5BA);
        var poisonBarAlternativeColor = reader.GetBgrColorAtAddress(0x0007B5AB);

        if (poisonBarMainColor.R != 255 || poisonBarMainColor.G != 0 || poisonBarMainColor.B != 0 ||
            poisonBarFadeColor.R != 0 || poisonBarFadeColor.G != 0 || poisonBarFadeColor.B != 0 ||
            poisonBarAlternativeColor.R != 0 || poisonBarAlternativeColor.G != 255 ||
            poisonBarAlternativeColor.B != 0 || gradientType != GradientType.Normal)
        {
            poisonBarMainColor = new ColorRgb()
            {
                R = poisonBarAlternativeColor.R,
                G = poisonBarAlternativeColor.G,
                B = poisonBarAlternativeColor.B,
            };

            ConstructBar(poisonBarInfo, poisonBarMainColor, poisonBarFadeColor, gradientType);
        }

        UpdateBarBackgroundColors(reader, poisonBarInfo);

        poisonBarInfo.Width = reader.ReadShortAt(0x0007B5C5).NullIf(DefaultBarWidth);
        poisonBarInfo.Height = reader.ReadByteAt(0x0007B5C3).NullIf(DefaultBarHeight);
        poisonBarInfo.IsAnimated = reader.CompareDataAtAddress(0x0007B5CC, [0x50, 0xD7]).NullIf(false);

        if (poisonBarInfo.HasAnyValue())
            return poisonBarInfo;

        return null;
    }

    private BarStyle? ReadAirBarInfo(BinaryReader reader, GradientType gradientType)
    {
        var airBarInfo = new BarStyle();

        var airBarMainColor = reader.GetBgrColorAtAddress(0x0007B565);
        var airBarFadeColor = reader.GetBgrColorAtAddress(0x0007B56D);

        if (airBarMainColor.R != 0 || airBarMainColor.G != 0 || airBarMainColor.B != 255 ||
            airBarFadeColor.R != 0 || airBarFadeColor.G != 0 || airBarFadeColor.B != 0 ||
            gradientType != GradientType.Normal)
            ConstructBar(airBarInfo, airBarMainColor, airBarFadeColor, gradientType);

        UpdateBarBackgroundColors(reader, airBarInfo);

        airBarInfo.Width = reader.ReadShortAt(0x0007B579).NullIf(DefaultBarWidth);
        airBarInfo.Height = reader.ReadByteAt(0x0007B575).NullIf(DefaultBarHeight);
        airBarInfo.XOffset = reader.ReadShortAt(0x0007B57F).NullIf(DefaultAirBarOffset);
        airBarInfo.IsAnimated = reader.CompareDataAtAddress(0x0007B587, [0x95, 0xD7]).NullIf(false);

        if (airBarInfo.HasAnyValue())
            return airBarInfo;

        return null;
    }

    private BarStyle? ReadSprintBarInfo(BinaryReader reader, GradientType gradientType)
    {
        var sprintBarInfo = new BarStyle();

        var sprintBarMainColor = reader.GetBgrColorAtAddress(0x0007B523);
        var sprintBarFadeColor = reader.GetBgrColorAtAddress(0x0007B528);

        if (sprintBarMainColor.R != 0 || sprintBarMainColor.G != 255 || sprintBarMainColor.B != 0 ||
            sprintBarFadeColor.R != 0 || sprintBarFadeColor.G != 0 || sprintBarFadeColor.B != 0 ||
            gradientType != GradientType.Normal)
            ConstructBar(sprintBarInfo, sprintBarMainColor, sprintBarFadeColor, gradientType);

        UpdateBarBackgroundColors(reader, sprintBarInfo);

        sprintBarInfo.Width = reader.ReadShortAt(0x0007B538).NullIf(DefaultBarWidth);
        sprintBarInfo.Height = reader.ReadByteAt(0x0007B536).NullIf(DefaultBarHeight);
        sprintBarInfo.XOffset = reader.ReadShortAt(0x0007B531).NullIf(DefaultAirBarOffset);
        sprintBarInfo.IsAnimated = reader.CompareDataAtAddress(0x0007B541, [0xDB, 0xD7]).NullIf(false);

        if (sprintBarInfo.HasAnyValue())
            return sprintBarInfo;

        return null;
    }

    private BarStyle? ReadLoadingBarInfo(BinaryReader reader, GradientType gradientType)
    {
        var loadingBarInfo = new BarStyle();

        var loadingBarMainColor = reader.GetBgrColorAtAddress(0x0007B65A);
        var loadingBarFadeColor = reader.GetBgrColorAtAddress(0x0007B65F);

        if (loadingBarMainColor.R != 159 || loadingBarMainColor.G != 31 || loadingBarMainColor.B != 128 ||
            loadingBarFadeColor.R != 0 || loadingBarFadeColor.G != 0 || loadingBarFadeColor.B != 0 ||
            gradientType != GradientType.Normal)
            ConstructBar(loadingBarInfo, loadingBarMainColor, loadingBarFadeColor, gradientType);

        UpdateBarBackgroundColors(reader, loadingBarInfo);

        const short defaultLoadingBarWidth = 600;
        loadingBarInfo.Width = reader.ReadShortAt(0x0007B693).NullIf(defaultLoadingBarWidth);

        const byte defaultLoadingBarHeight = 15;
        loadingBarInfo.Height = reader.ReadByteAt(0x0007B68F).NullIf(defaultLoadingBarHeight);

        loadingBarInfo.Hidden = reader.IsNopAtRange(0x0007B601, 0x0007B604).NullIf(false);

        if (loadingBarInfo.HasAnyValue())
            return loadingBarInfo;

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
}
