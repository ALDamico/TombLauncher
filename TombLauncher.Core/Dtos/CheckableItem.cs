namespace TombLauncher.Core.Dtos;

public class CheckableItem<T> : IEquatable<T> where T: IEquatable<T>
{
    public bool IsEnabled { get; set; }
    public T Value { get; set; }

    protected bool Equals(CheckableItem<T> other)
    {
        return IsEnabled == other.IsEnabled && EqualityComparer<T>.Default.Equals(Value, other.Value);
    }

    public bool Equals(T other)
    {
        return EqualityComparer<T>.Default.Equals(Value, other);
    }

    public override bool Equals(object obj)
    {
        if (obj is null) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((CheckableItem<T>)obj);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(IsEnabled, Value);
    }
}