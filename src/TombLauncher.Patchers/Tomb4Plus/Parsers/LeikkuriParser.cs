using TombLauncher.Contracts.Extensions;
using TombLauncher.Patchers.Tomb4Plus.Extensions;
using TombLauncher.Patchers.Tomb4Plus.Models;

namespace TombLauncher.Patchers.Tomb4Plus.Parsers;

public class LeikkuriParser
{
    public void ExtractFontDataFromExe(string exeFilePath, FontInfo? fontInfo)
    {
        using var reader = new BinaryReader(File.OpenRead(exeFilePath));
        fontInfo ??= new FontInfo();

        reader.Seek(LeikkuriDataTables.FontTableAddress);
        var fontTable = new List<FontGlyph>();

        // Starts at 0x000ACD98
        // Ends at 0x000AD437
        for (var i = 0; i < LeikkuriDataTables.DefaultFontTable.Count; i++)
        {
            var u = (float)Math.Round(reader.ReadSingle(), LeikkuriDataTables.RoundingPoint, MidpointRounding.ToEven);
            var v = (float)Math.Round(reader.ReadSingle(), LeikkuriDataTables.RoundingPoint, MidpointRounding.ToEven);

            var w = reader.ReadInt16();
            var h = reader.ReadInt16();
            var topShade = reader.ReadSByte();
            var bottomShade = reader.ReadSByte();

            var fontGlyph = new FontGlyph()
            {
                U = u,
                V = v,
                W = w,
                H = h,
                TopShade = topShade,
                BottomShade = bottomShade
            };
            
            fontTable.Add(fontGlyph);
        }
        
        // font_table.txt doesn't seem necessary to Tomb4Plus, so that code portion isn't being ported.

        reader.Seek(LeikkuriDataTables.WidthAddress);

        fontInfo.CustomGlyphScaleWidth = reader.ReadInt32().NullIf(512);

        reader.Seek(LeikkuriDataTables.HeightAddress);
        fontInfo.CustomGlyphScaleHeight = reader.ReadInt32().NullIf(240);

        reader.Seek(LeikkuriDataTables.VerticalSpacingAddress);
        fontInfo.CustomVerticalSpacing = ((float?)Math
            .Round(reader.ReadSingle(), LeikkuriDataTables.RoundingPoint, MidpointRounding.ToEven)).NullIf(0.075F);
        
        reader.Seek(LeikkuriDataTables.CompressedTextFactorAddress);
        fontInfo.CustomCompressedTextFactor = ((float?)Math.Round(reader.ReadSingle(), LeikkuriDataTables.RoundingPoint,
                MidpointRounding.ToEven)).NullIf(0.75F);

        if (LeikkuriDataTables.DefaultFontTable.Where((g, i) => g != fontTable[i]).Any())
            fontInfo.CustomFontTable = fontTable;
    }
}