namespace TombLauncher.Patchers.Tomb4Plus.Models;

public class FontInfo
{
    public byte? TextOrCriticalBarBlinkInterval { get; set; }
    public ColorRgb? MainFontMainColor { get; set; }
    public ColorRgb? MainFontFadeColor { get; set; }
    public ColorRgb? OptionsTitleFontMainColor { get; set; }
    public ColorRgb? OptionsTitleFontFadeColor { get; set; }
    public ColorRgb? InventoryTitleFontMainColor { get; set; }
    public ColorRgb? InventoryTitleFontFadeColor { get; set; }
    public ColorRgb? InventoryItemFontMainColor { get; set; }
    public ColorRgb? InventoryItemFontFadeColor { get; set; }
    public int? CustomGlyphScaleWidth { get; set; }
    public int? CustomGlyphScaleHeight { get; set; }
    public float? CustomVerticalSpacing { get; set; }
    public float? CustomCompressedTextFactor { get; set; }
    public List<FontGlyph>? CustomFontTable { get; set; }
    
    public bool HasAnyValue() =>
        TextOrCriticalBarBlinkInterval != null ||
        MainFontMainColor != null ||
        MainFontFadeColor != null ||
        OptionsTitleFontMainColor != null ||
        OptionsTitleFontFadeColor != null ||
        InventoryTitleFontMainColor != null ||
        InventoryTitleFontFadeColor != null ||
        InventoryItemFontMainColor != null ||
        InventoryItemFontFadeColor != null ||
        CustomGlyphScaleWidth != null ||
        CustomGlyphScaleHeight != null ||
        CustomVerticalSpacing != null ||
        CustomCompressedTextFactor != null ||
        CustomFontTable != null;

    public static ColorRgb DefaultFontMainColor { get; } = new ColorRgb(128, 128, 128);
    public static ColorRgb DefaultOptionsMainColor { get; } = new ColorRgb(192, 128, 64);
    public static ColorRgb DefaultOptionsFadeColor { get; } = new ColorRgb(64, 16, 0);
    public static ColorRgb DefaultInventoryFontMainColor { get; } = new ColorRgb(224, 192, 0);
    public static ColorRgb DefaultInventoryFontFadeColor { get; } = new ColorRgb(64, 32, 0);
    public static ColorRgb DefaultInventoryItemFontFadeColor { get; } = new ColorRgb(16, 16, 16);
}