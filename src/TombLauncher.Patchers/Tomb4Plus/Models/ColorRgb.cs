namespace TombLauncher.Patchers.Tomb4Plus.Models;

public class ColorRgb
{
    public ColorRgb()
    {
        
    }
    
    public ColorRgb(byte r, byte g, byte b)
    {
        R = r;
        G = g;
        B = b;
    }
    protected bool Equals(ColorRgb other)
    {
        return R == other.R && G == other.G && B == other.B;
    }

    public override bool Equals(object? obj)
    {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((ColorRgb)obj);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(R, G, B);
    }

    public byte R { get; init; }
    public byte G { get; init; }
    public byte B { get; init; }

    public static bool operator ==(ColorRgb? rgb, ColorRgb? other)
    {
        return rgb?.R == other?.R && rgb?.G == other?.G && rgb?.B == other?.B;
    }

    public static bool operator !=(ColorRgb? rgb, ColorRgb? other)
    {
        return !(rgb == other);
    }
}