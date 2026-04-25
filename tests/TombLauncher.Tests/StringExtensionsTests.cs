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

    // --- EnsureStartsWith ---

    [Theory]
    // Relative URL → prepend BaseUrl with separator
    [InlineData("/level.php?id=1", "https://trle.net", '/', "https://trle.net/level.php?id=1")]
    // Already absolute URL, same domain → unchanged
    [InlineData("https://trle.net/level.php?id=1", "https://trle.net", '/', "https://trle.net/level.php?id=1")]
    // Absolute URL with www., prefix without → unchanged (original bug)
    [InlineData("https://www.trle.net/scadm/trle_dl.php?lid=3674", "https://trle.net", '/', "https://www.trle.net/scadm/trle_dl.php?lid=3674")]
    // Double slash on path → trimmed correctly
    [InlineData("//level.php", "https://trle.net", '/', "https://trle.net/level.php")]
    // BaseUrl with trailing slash → no double slash in result
    [InlineData("/level.php", "https://trle.net/", '/', "https://trle.net/level.php")]
    public void EnsureStartsWith_WithSeparator_ReturnsExpectedUrl(string s, string prefix, char sep, string expected)
    {
        Assert.Equal(expected, s.EnsureStartsWith(prefix, sep));
    }

    [Theory]
    // No separator: relative URL → direct concat
    [InlineData("/level.php", "https://trle.net", "https://trle.net/level.php")]
    // No separator: absolute URL → unchanged
    [InlineData("https://trle.net/level.php", "https://trle.net", "https://trle.net/level.php")]
    public void EnsureStartsWith_WithoutSeparator_ReturnsExpectedUrl(string s, string prefix, string expected)
    {
        Assert.Equal(expected, s.EnsureStartsWith(prefix));
    }

    [Fact]
    public void EnsureStartsWith_NullS_ReturnsPrefix()
    {
        // null s → empty string joined with prefix → trailing separator
        Assert.Equal("https://trle.net/", ((string?)null).EnsureStartsWith("https://trle.net", '/'));
    }

    [Fact]
    public void EnsureStartsWith_NullPrefix_ReturnsS()
    {
        // null prefix → empty string joined with separator → "/level.php"
        Assert.Equal("/level.php", "/level.php".EnsureStartsWith(null, '/'));
    }
}

