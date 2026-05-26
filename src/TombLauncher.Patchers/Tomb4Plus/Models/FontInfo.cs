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
}