using System;
using System.Globalization;
using System.Text.RegularExpressions;
using Avalonia.Data.Converters;

namespace TombLauncher.ValueConverters;

/// <summary>
/// Preprocesses game descriptions before Markdown rendering:
/// - Converts TRCustoms custom BBCode tags to HTML spans with appropriate colors
/// - Converts single-tilde strikethrough (~text~) to Markdig double-tilde (~~text~~)
/// Safe to apply on any source (TRLE, AspideTR): regex has no match → original text returned.
/// </summary>
public partial class MarkdownDescriptionConverter : IValueConverter
{
    public static readonly MarkdownDescriptionConverter Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not string text) return null;
        return Preprocess(text);
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();

    private static string Preprocess(string text)
    {
        text = PickupsRegex().Replace(text, "<span style=\"color:#4CAF50\">$1</span>");
        text = SecretsRegex().Replace(text, "<span style=\"color:#FFC107\">$1</span>");
        text = TrapsRegex().Replace(text, "<span style=\"color:#8B0000\">$1</span>");
        text = EnemiesRegex().Replace(text, "<span style=\"color:#D32F2F\">$1</span>");
        text = ObjectsRegex().Replace(text, "<span style=\"color:#1976D2\">$1</span>");
        text = CenterRegex().Replace(text, "<div style=\"text-align:center\">$1</div>");
        // Single-tilde strikethrough (~text~) → Markdig double-tilde (~~text~~)
        text = StrikethroughRegex().Replace(text, "~~$1~~");
        return text;
    }

    // TRCustoms custom color tags
    [GeneratedRegex(@"\[p\](.*?)\[/p\]", RegexOptions.Singleline)]
    private static partial Regex PickupsRegex();

    [GeneratedRegex(@"\[s\](.*?)\[/s\]", RegexOptions.Singleline)]
    private static partial Regex SecretsRegex();

    [GeneratedRegex(@"\[t\](.*?)\[/t\]", RegexOptions.Singleline)]
    private static partial Regex TrapsRegex();

    [GeneratedRegex(@"\[e\](.*?)\[/e\]", RegexOptions.Singleline)]
    private static partial Regex EnemiesRegex();

    [GeneratedRegex(@"\[o\](.*?)\[/o\]", RegexOptions.Singleline)]
    private static partial Regex ObjectsRegex();

    [GeneratedRegex(@"\[center\](.*?)\[/center\]", RegexOptions.Singleline)]
    private static partial Regex CenterRegex();

    // Matches ~text~ (single tilde) but NOT ~~text~~ (double tilde)
    [GeneratedRegex(@"(?<!~)~(?!~)(.*?)(?<!~)~(?!~)", RegexOptions.Singleline)]
    private static partial Regex StrikethroughRegex();
}
