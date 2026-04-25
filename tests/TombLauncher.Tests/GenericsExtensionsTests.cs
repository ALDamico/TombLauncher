using TombLauncher.Core.Extensions;

namespace TombLauncher.Tests;

public class GenericsExtensionsTests
{
    [Fact]
    public void Coalesce_FirstIsNonDefault_ReturnsFirst()
    {
        var result = "hello".Coalesce("world");
        Assert.Equal("hello", result);
    }

    [Fact]
    public void Coalesce_FirstIsNull_ReturnsSecond()
    {
        string? first = null;
        var result = first.Coalesce("fallback");
        Assert.Equal("fallback", result);
    }

    [Fact]
    public void Coalesce_AllNull_ReturnsDefault()
    {
        string? first = null;
        var result = first.Coalesce(null, null);
        Assert.Null(result);
    }

    [Fact]
    public void Coalesce_SkipsNullsAndReturnsFirstNonDefault()
    {
        string? first = null;
        var result = first.Coalesce(null, "third", "fourth");
        Assert.Equal("third", result);
    }

    [Fact]
    public void Coalesce_SkipsNullsAndReturnsFirstNonDefault2()
    {
        string? first = null;
        var result = first.Coalesce(null, null, "third", "fourth");
        Assert.Equal("third", result);
    }

    [Fact]
    public void Coalesce_NullableValueType_ReturnsFirstNonNull()
    {
        int? first = null;
        var result = first.Coalesce(null, 42);
        Assert.Equal(42, result);
    }

    [Fact]
    public void Coalesce_NullableValueType_FirstHasValue_ReturnsFirst()
    {
        int? first = 7;
        var result = first.Coalesce(42);
        Assert.Equal(7, result);
    }
}
