using TombLauncher.Contracts.Extensions;
using TombLauncher.Patchers.Tomb4Plus.Extensions;
using TombLauncher.Patchers.Tomb4Plus.Models;

namespace TombLauncher.Patchers.Tomb4Plus.Extractors;

public partial class TrlePatchExtractor
{
    private FontInfo? ReadFontInfo(BinaryReader reader)
    {
        var fontInfo = new FontInfo();

        const byte defaultCriticalBlinkInterval = 5;
        fontInfo.TextOrCriticalBarBlinkInterval =
            reader.ReadByteAt(0x000521B0).NullIf(defaultCriticalBlinkInterval);

        fontInfo.MainFontMainColor =
            reader.GetRgbColorAtAddress(0x000ADEF8).NullIf(FontInfo.DefaultFontMainColor);
        fontInfo.MainFontFadeColor =
            reader.GetRgbColorAtAddress(0x000ADEFC).NullIf(FontInfo.DefaultFontMainColor);
        fontInfo.OptionsTitleFontMainColor =
            reader.GetRgbColorAtAddress(0x000ADF18).NullIf(FontInfo.DefaultOptionsMainColor);
        fontInfo.OptionsTitleFontFadeColor =
            reader.GetRgbColorAtAddress(0x000ADF1C).NullIf(FontInfo.DefaultOptionsFadeColor);
        fontInfo.InventoryTitleFontMainColor =
            reader.GetRgbColorAtAddress(0x000ADF28).NullIf(FontInfo.DefaultInventoryFontMainColor);
        fontInfo.InventoryTitleFontFadeColor =
            reader.GetRgbColorAtAddress(0x000ADF2C).NullIf(FontInfo.DefaultInventoryFontFadeColor);
        fontInfo.InventoryItemFontMainColor =
            reader.GetRgbColorAtAddress(0x000ADF10).NullIf(FontInfo.DefaultFontMainColor);
        fontInfo.InventoryItemFontFadeColor =
            reader.GetRgbColorAtAddress(0x000ADF14).NullIf(FontInfo.DefaultInventoryItemFontFadeColor);

        if (fontInfo.HasAnyValue())
            return fontInfo;

        return null;
    }
}
