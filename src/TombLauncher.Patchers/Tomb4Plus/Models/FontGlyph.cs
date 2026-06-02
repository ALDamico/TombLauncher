// ReSharper disable CompareOfFloatsByEqualityOperator
namespace TombLauncher.Patchers.Tomb4Plus.Models;

public class FontGlyph
{
    public float U { get; set; }
    public float V { get; set; }
    public short W { get; set; }
    public short H { get; set; }
    public short YOffset { get; set; }
    public sbyte TopShade { get; set; }
    public sbyte BottomShade { get; set; }

    public override bool Equals(object? obj) => obj is FontGlyph other && this == other;

    // ReSharper disable NonReadonlyMemberInGetHashCode
    public override int GetHashCode() => HashCode.Combine(U, V, W, H, YOffset, TopShade, BottomShade);

    public static bool operator ==(FontGlyph? first, FontGlyph? second)
    {
        return first?.U == second?.U &&
               first?.V == second?.V &&
               first?.W == second?.W &&
               first?.H == second?.H &&
               first?.YOffset == second?.YOffset &&
               first?.TopShade == second?.TopShade &&
               first?.BottomShade == second?.BottomShade;
    }

    public static bool operator !=(FontGlyph? first, FontGlyph? second)
    {
        return !(first == second);
    }
}