using TombLauncher.Core.Extensions;

namespace TombLauncher.Tests;

public class StringExtensionsTests
{
    // --- RemoveDiacritics ---

    [Theory]
    [InlineData("café", "cafe")]
    [InlineData("naïve", "naive")]
    [InlineData("résumé", "resume")]
    [InlineData("hello", "hello")]
    [InlineData("", "")]
    public void RemoveDiacritics_RemovesAccentsCorrectly(string input, string expected)
    {
        Assert.Equal(expected, input.RemoveDiacritics());
    }

    // --- RemoveIncidentals ---

    [Theory]
    [InlineData("Title (sub)", "Title ")]
    [InlineData("Title [v2]", "Title ")]
    [InlineData("Title {draft}", "Title ")]
    [InlineData("A (x) B [y]", "A  B ")]
    [InlineData("Simple title", "Simple title")]
    [InlineData("Title (wrong]", "Title (wrong]")]
    public void RemoveIncidentals_RemovesMatchingBracketPairs(string input, string expected)
    {
        Assert.Equal(expected, input.RemoveIncidentals());
    }

    // --- GetNullTerminatedString ---

    [Fact]
    public void GetNullTerminatedString_StopsAtNullChar()
    {
        byte[] arr = [72, 105, 0, 88, 88]; // "Hi\0XX"
        Assert.Equal("Hi", arr.GetNullTerminatedString());
    }

    [Fact]
    public void GetNullTerminatedString_NoNullChar_ReturnsFullString()
    {
        byte[] arr = [65, 66, 67]; // "ABC"
        Assert.Equal("ABC", arr.GetNullTerminatedString());
    }

    [Fact]
    public void GetNullTerminatedString_WithSliceEnd_TruncatesAtSliceEnd()
    {
        byte[] arr = [65, 66, 67, 68]; // "ABCD"
        Assert.Equal("AB", arr.GetNullTerminatedString(2));
    }

    [Fact]
    public void GetNullTerminatedString_NullCharBeforeSliceEnd_NullWins()
    {
        byte[] arr = [65, 0, 67]; // "A\0C"
        Assert.Equal("A", arr.GetNullTerminatedString(3));
    }

    [Fact]
    public void GetNullTerminatedString_EmptyArray_ReturnsEmpty()
    {
        byte[] arr = [];
        Assert.Equal("", arr.GetNullTerminatedString());
    }

    [Fact]
    public void GetNullTerminatedString_NullCharAtStart_ReturnsEmpty()
    {
        byte[] arr = [0, 65, 66];
        Assert.Equal("", arr.GetNullTerminatedString());
    }

    [Fact]
    public void GetNullTerminatedString_AllNulls_ReturnsEmpty()
    {
        byte[] arr = [0, 0, 0];
        Assert.Equal("", arr.GetNullTerminatedString());
    }

    [Fact]
    public void GetNullTerminatedString_MultipleNulls_FirstNullWins()
    {
        byte[] arr = [65, 0, 66, 0]; // "A\0B\0"
        Assert.Equal("A", arr.GetNullTerminatedString());
    }

    [Fact]
    public void GetNullTerminatedString_SliceEndBeyondLength_ClampsToArrayLength()
    {
        byte[] arr = [65, 66]; // "AB"
        Assert.Equal("AB", arr.GetNullTerminatedString(10));
    }

    [Fact]
    public void GetNullTerminatedString_SliceEndZero_ReturnsEmpty()
    {
        byte[] arr = [65, 66, 67]; // "ABC"
        Assert.Equal("", arr.GetNullTerminatedString(0));
    }
}
