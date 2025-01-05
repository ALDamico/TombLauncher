namespace TombLauncher.Core.Dtos;

public class DownloaderConfiguration : IEquatable<DownloaderConfiguration>, IEqualityComparer<DownloaderConfiguration>
{
    public string DisplayName { get; set; }
    public string BaseUrl { get; set; }
    public bool IsEnabled { get; set; }
    public int Priority { get; set; }
    public string ClassName { get; set; }
    public string SupportedFeatures { get; set; }

    public bool Equals(DownloaderConfiguration other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return DisplayName == other.DisplayName && BaseUrl == other.BaseUrl && IsEnabled == other.IsEnabled &&
               Priority == other.Priority && ClassName == other.ClassName &&
               SupportedFeatures == other.SupportedFeatures;
    }

    public override bool Equals(object obj)
    {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((DownloaderConfiguration)obj);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(DisplayName, BaseUrl, IsEnabled, Priority, ClassName, SupportedFeatures);
    }

    public bool Equals(DownloaderConfiguration x, DownloaderConfiguration y)
    {
        if (ReferenceEquals(x, y)) return true;
        if (x is null) return false;
        if (y is null) return false;
        if (x.GetType() != y.GetType()) return false;
        return x.DisplayName == y.DisplayName && x.BaseUrl == y.BaseUrl && x.IsEnabled == y.IsEnabled &&
               x.Priority == y.Priority && x.ClassName == y.ClassName && x.SupportedFeatures == y.SupportedFeatures;
    }

    public int GetHashCode(DownloaderConfiguration obj)
    {
        return HashCode.Combine(obj.DisplayName, obj.BaseUrl, obj.IsEnabled, obj.Priority, obj.ClassName,
            obj.SupportedFeatures);
    }
}