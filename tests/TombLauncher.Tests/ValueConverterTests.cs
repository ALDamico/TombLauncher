using System.Globalization;
using IconPacks.Avalonia.RemixIcon;
using TombLauncher.ValueConverters;

namespace TombLauncher.Tests;

public class ValueConverterTests
{
    private static readonly CultureInfo Culture = CultureInfo.InvariantCulture;

    // --- GreaterThanZeroToBoolConverter ---

    [Theory]
    [InlineData(1, true)]
    [InlineData(0, false)]
    [InlineData(-1, false)]
    [InlineData(0.5, true)]
    [InlineData(0.0, false)]
    [InlineData(-0.1, false)]
    [InlineData((long)100, true)]
    [InlineData((long)0, false)]
    [InlineData((byte)1, true)]
    [InlineData((byte)0, false)]
    public void GreaterThanZero_ReturnsExpected(object value, bool expected)
    {
        var converter = new GreaterThanZeroToBoolConverter();
        Assert.Equal(expected, converter.Convert(value, typeof(bool), null, Culture));
    }

    [Fact]
    public void GreaterThanZero_NullReturnsFalse()
    {
        var converter = new GreaterThanZeroToBoolConverter();
        Assert.Equal(false, converter.Convert(null, typeof(bool), null, Culture));
    }

    [Fact]
    public void GreaterThanZero_NonNumericReturnsFalse()
    {
        var converter = new GreaterThanZeroToBoolConverter();
        Assert.Equal(false, converter.Convert(new object(), typeof(bool), null, Culture));
    }

    // --- CollectionToVisibilityConverter ---

    [Fact]
    public void CollectionToVisibility_EmptyCollection_ReturnsTrue()
    {
        var converter = new CollectionToVisibilityConverter();
        Assert.Equal(true, converter.Convert(Array.Empty<int>(), typeof(bool), null, Culture));
    }

    [Fact]
    public void CollectionToVisibility_NonEmptyCollection_ReturnsFalse()
    {
        var converter = new CollectionToVisibilityConverter();
        Assert.Equal(false, converter.Convert(new[] { 1, 2 }, typeof(bool), null, Culture));
    }

    [Fact]
    public void CollectionToVisibility_Inverted_EmptyReturns_False()
    {
        var converter = new CollectionToVisibilityConverter { Invert = true };
        Assert.Equal(false, converter.Convert(Array.Empty<int>(), typeof(bool), null, Culture));
    }

    [Fact]
    public void CollectionToVisibility_Inverted_NonEmptyReturns_True()
    {
        var converter = new CollectionToVisibilityConverter { Invert = true };
        Assert.Equal(true, converter.Convert(new[] { 1 }, typeof(bool), null, Culture));
    }

    [Fact]
    public void CollectionToVisibility_NullReturnsFalse()
    {
        var converter = new CollectionToVisibilityConverter();
        Assert.Equal(false, converter.Convert(null, typeof(bool), null, Culture));
    }

    // --- IsSmallerOrEqualConverter ---

    [Fact]
    public void IsSmallerOrEqual_SmallerReturnsTrue()
    {
        var converter = new IsSmallerOrEqualConverter();
        var result = converter.Convert(new List<object?> { 2, 5 }, typeof(bool), null, Culture);
        Assert.Equal(true, result);
    }

    [Fact]
    public void IsSmallerOrEqual_EqualReturnsTrue()
    {
        var converter = new IsSmallerOrEqualConverter();
        var result = converter.Convert(new List<object?> { 3, 3 }, typeof(bool), null, Culture);
        Assert.Equal(true, result);
    }

    [Fact]
    public void IsSmallerOrEqual_LargerReturnsFalse()
    {
        var converter = new IsSmallerOrEqualConverter();
        var result = converter.Convert(new List<object?> { 5, 2 }, typeof(bool), null, Culture);
        Assert.Equal(false, result);
    }

    [Fact]
    public void IsSmallerOrEqual_WrongCountThrows()
    {
        var converter = new IsSmallerOrEqualConverter();
        Assert.Throws<ArgumentException>(() =>
            converter.Convert(new List<object?> { 1 }, typeof(bool), null, Culture));
    }

    // --- FileSizeConverter ---

    [Theory]
    [InlineData(0L, 0.0, "bytes")]
    [InlineData(512L, 512.0, "bytes")]
    [InlineData(1024L, 1024.0, "bytes")]
    [InlineData(1536L, 1.5, "kB")]
    [InlineData(1048576L, 1024.0, "kB")]
    [InlineData(1572864L, 1.5, "MB")]
    public void FileSizeConverter_FormatsCorrectly(long bytes, double expectedValue, string expectedUnit)
    {
        var converter = new FileSizeConverter();
        var result = (string?)converter.Convert(bytes, typeof(string), null, Culture);
        Assert.Equal($"{expectedValue:F2} {expectedUnit}", result);
    }

    [Fact]
    public void FileSizeConverter_NullReturnsEmpty()
    {
        var converter = new FileSizeConverter();
        Assert.Equal(string.Empty, converter.Convert(null, typeof(string), null, Culture));
    }

    // --- TransferSpeedConverter ---

    [Theory]
    [InlineData(512.0, 512.0, "B/s")]
    [InlineData(1024.0, 1024.0, "B/s")]
    [InlineData(1536.0, 1.5, "kB/s")]
    [InlineData(1048576.0, 1024.0, "kB/s")]
    [InlineData(1572864.0, 1.5, "MB/s")]
    public void TransferSpeedConverter_FormatsCorrectly(double speed, double expectedValue, string expectedUnit)
    {
        var converter = new TransferSpeedConverter();
        var result = (string?)converter.Convert(speed, typeof(string), null, Culture);
        Assert.Equal($"{expectedValue:F2} {expectedUnit}", result);
    }

    [Fact]
    public void TransferSpeedConverter_NullReturnsNull()
    {
        var converter = new TransferSpeedConverter();
        Assert.Null(converter.Convert(null, typeof(string), null, Culture));
    }

    private const PackIconRemixIconKind TrueIcon = PackIconRemixIconKind.CheckLine;
    private const PackIconRemixIconKind FalseIcon = PackIconRemixIconKind.DeleteBackFill;

    [Theory]
    [InlineData(true, TrueIcon)]
    [InlineData(false, FalseIcon)]
    [InlineData(";P", null)]
    public void BooleanToIconConverter_Convert_ConvertsCorrectly(object value, PackIconRemixIconKind? expectedValue)
    {
        var converter = new BooleanToIconConverter(){TrueValue = TrueIcon, FalseValue = FalseIcon};
        Assert.Equal(expectedValue, converter.Convert(value, typeof(PackIconRemixIconKind), null, Culture));
    }
    
    [Theory]
    [InlineData(TrueIcon, true)]
    [InlineData( FalseIcon, false)]
    [InlineData(null, false)]
    public void BooleanToIconConverter_ConvertBack_ConvertsCorrectly(PackIconRemixIconKind? value, bool expectedValue)
    {
        var converter = new BooleanToIconConverter(){TrueValue = TrueIcon, FalseValue = FalseIcon};
        Assert.Equal(expectedValue, converter.ConvertBack(value, typeof(bool), null, Culture));
    }
}
