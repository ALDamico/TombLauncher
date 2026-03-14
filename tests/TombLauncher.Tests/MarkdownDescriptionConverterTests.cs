using System.Globalization;
using TombLauncher.ValueConverters;

namespace TombLauncher.Tests;

public class MarkdownDescriptionConverterTests
{
    private static readonly MarkdownDescriptionConverter Converter = new();
    private static readonly CultureInfo Culture = CultureInfo.InvariantCulture;

    private string? Convert(string? input)
        => (string?)Converter.Convert(input, typeof(string), null, Culture);

    // --- Null / passthrough ---

    [Fact]
    public void Convert_NullInput_ReturnsNull()
    {
        Assert.Null(Convert(null));
    }

    [Fact]
    public void Convert_PlainText_ReturnsUnchanged()
    {
        const string text = "No special tags here.";
        Assert.Equal(text, Convert(text));
    }

    [Fact]
    public void Convert_EmptyString_ReturnsEmpty()
    {
        Assert.Equal(string.Empty, Convert(string.Empty));
    }

    // --- BBCode color tags ---

    [Theory]
    [InlineData("[p]Pickups[/p]", "<span style=\"color:#4CAF50\">Pickups</span>")]
    [InlineData("[s]Secrets[/s]", "<span style=\"color:#FFC107\">Secrets</span>")]
    [InlineData("[t]Traps[/t]", "<span style=\"color:#8B0000\">Traps</span>")]
    [InlineData("[e]Enemies[/e]", "<span style=\"color:#D32F2F\">Enemies</span>")]
    [InlineData("[o]Objects[/o]", "<span style=\"color:#1976D2\">Objects</span>")]
    // Edge cases
    [InlineData("[p][/p]", "<span style=\"color:#4CAF50\"></span>")]          // empty content
    [InlineData("[P]text[/P]", "[P]text[/P]")]                                 // case-sensitive: no match
    [InlineData("[p]  spaces  [/p]", "<span style=\"color:#4CAF50\">  spaces  </span>")] // whitespace content
    public void Convert_ColorTag_ProducesColoredSpan(string input, string expected)
    {
        Assert.Equal(expected, Convert(input));
    }

    [Fact]
    public void Convert_CenterTag_ProducesDivCenter()
    {
        const string input = "[center]Centered text[/center]";
        const string expected = "<div style=\"text-align:center\">Centered text</div>";
        Assert.Equal(expected, Convert(input));
    }

    // --- Strikethrough ---

    [Fact]
    public void Convert_SingleTilde_ConvertsToDoubleForMarkdig()
    {
        Assert.Equal("~~crossed~~", Convert("~crossed~"));
    }

    [Fact]
    public void Convert_DoubleTilde_LeftUnchanged()
    {
        const string text = "~~already strikethrough~~";
        Assert.Equal(text, Convert(text));
    }

    [Fact]
    public void Convert_SingleTildeInSentence_OnlyTildeConverted()
    {
        Assert.Equal("normal ~~crossed~~ normal", Convert("normal ~crossed~ normal"));
    }

    [Fact]
    public void Convert_MultipleSingleTildes_AllConverted()
    {
        Assert.Equal("~~a~~ and ~~b~~", Convert("~a~ and ~b~"));
    }

    [Fact]
    public void Convert_UnclosedTilde_LeftUnchanged()
    {
        const string text = "~orphan";
        Assert.Equal(text, Convert(text));
    }

    // --- Multiline content inside tags ---

    [Fact]
    public void Convert_MultilinePickupTag_Converted()
    {
        const string input = "[p]line1\nline2[/p]";
        const string expected = "<span style=\"color:#4CAF50\">line1\nline2</span>";
        Assert.Equal(expected, Convert(input));
    }

    [Fact]
    public void Convert_CenterTag_WithNewlines_Converted()
    {
        const string input = "[center]line1\nline2[/center]";
        const string expected = "<div style=\"text-align:center\">line1\nline2</div>";
        Assert.Equal(expected, Convert(input));
    }

    [Fact]
    public void Convert_RealisticMultilineDescription_AllTagsConverted()
    {
        // Simulates a real TRCustoms description with markdown paragraphs,
        // BBCode tags spanning lines, strikethrough, and center.
        const string input =
            "A classic adventure set in Egypt.\n\n" +
            "[center]**My Custom Level**[/center]\n\n" +
            "## Stats\n\n" +
            "[e]Enemies: 12 mummies and 3 demigods[/e]\n" +
            "[p]Pickups: medipack, shotgun, ~revolver~[/p]\n" +
            "[s]Secrets: 3[/s]\n" +
            "[t]Traps: rolling boulders[/t]";

        const string expected =
            "A classic adventure set in Egypt.\n\n" +
            "<div style=\"text-align:center\">**My Custom Level**</div>\n\n" +
            "## Stats\n\n" +
            "<span style=\"color:#D32F2F\">Enemies: 12 mummies and 3 demigods</span>\n" +
            "<span style=\"color:#4CAF50\">Pickups: medipack, shotgun, ~~revolver~~</span>\n" +
            "<span style=\"color:#FFC107\">Secrets: 3</span>\n" +
            "<span style=\"color:#8B0000\">Traps: rolling boulders</span>";

        Assert.Equal(expected, Convert(input));
    }

    // --- Multiple tags in one string ---

    [Fact]
    public void Convert_MultipleDifferentTags_AllConverted()
    {
        const string input = "[e]Enemies: 5[/e] and [p]Pickups: 3[/p]";
        const string expected = "<span style=\"color:#D32F2F\">Enemies: 5</span> and <span style=\"color:#4CAF50\">Pickups: 3</span>";
        Assert.Equal(expected, Convert(input));
    }

    [Fact]
    public void Convert_SameTagRepeated_AllConverted()
    {
        const string input = "[s]Secret 1[/s] plus [s]Secret 2[/s]";
        const string expected = "<span style=\"color:#FFC107\">Secret 1</span> plus <span style=\"color:#FFC107\">Secret 2</span>";
        Assert.Equal(expected, Convert(input));
    }

    // --- Unclosed/unmatched tags are left as-is ---

    [Fact]
    public void Convert_UnclosedTag_LeftAsIs()
    {
        const string input = "[e]Unclosed enemy tag";
        Assert.Equal(input, Convert(input));
    }

    [Fact]
    public void Convert_UnmatchedCloseTag_LeftAsIs()
    {
        const string input = "No opening[/p]here";
        Assert.Equal(input, Convert(input));
    }

    // --- Out-of-order / crossed tags ---

    [Fact]
    public void Convert_CrossedTags_LazyMatchProducesNestedSpans()
    {
        // [s][t]blah[/s][/t]: SecretsRegex matches [s][t]blah[/s] lazily first,
        // then TrapsRegex matches [t]blah</span>[/t] in the resulting string.
        // End result: nested spans — malformed but predictable.
        const string input = "[s][t]blah[/s][/t]";
        const string expected =
            "<span style=\"color:#FFC107\"><span style=\"color:#8B0000\">blah</span></span>";
        Assert.Equal(expected, Convert(input));
    }

    [Fact]
    public void Convert_IdenticalNestedTags_OnlyOuterPairMatched()
    {
        // Regex does a single left-to-right scan: [s] at pos 0 matches lazily to
        // the FIRST [/s]. Capture = "[s]inner", so the inner [s] is literal in the span.
        // The trailing [/s] is left as-is (no preceding [s] after the replacement).
        const string input = "[s][s]inner[/s][/s]";
        const string expected = "<span style=\"color:#FFC107\">[s]inner</span>[/s]";
        Assert.Equal(expected, Convert(input));
    }

    // --- ConvertBack is not supported ---

    [Fact]
    public void ConvertBack_ThrowsNotSupportedException()
    {
        Assert.Throws<NotSupportedException>(() =>
            Converter.ConvertBack("text", typeof(string), null, Culture));
    }
}
