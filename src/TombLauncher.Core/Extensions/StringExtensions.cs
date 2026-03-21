using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace TombLauncher.Core.Extensions;

public static partial class StringExtensions
{
    public static string Remove(this string s, string toRemove)
    {
        return s.Replace(toRemove, string.Empty);
    }

    public static string RemoveDiacritics(this string text)
    {
        var normalizedString = text.Normalize(NormalizationForm.FormD);
        var stringBuilder = new StringBuilder(capacity: normalizedString.Length);

        for (int i = 0; i < normalizedString.Length; i++)
        {
            char c = normalizedString[i];
            var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
            if (unicodeCategory != UnicodeCategory.NonSpacingMark)
            {
                stringBuilder.Append(c);
            }
        }

        return stringBuilder
            .ToString()
            .Normalize(NormalizationForm.FormC);
    }

    public static string RemoveIncidentals(this string text)
    {
        return IncidentalsRegex().Replace(text, string.Empty);
    }

    [GeneratedRegex(@"\(.*?\)|\[.*?\]|\{.*?\}")]
    private static partial Regex IncidentalsRegex();

    public static string? NullIfEmpty(this string s)
    {
        return string.IsNullOrWhiteSpace(s) ? null : s;
    }

    public static bool IsNullOrEmpty(this string? s)
    {
        return string.IsNullOrEmpty(s);
    }

    public static bool IsNotNullOrEmpty(this string? s)
    {
        return !s.IsNullOrEmpty();
    }

    public static bool IsNullOrWhiteSpace(this string? s)
    {
        return string.IsNullOrWhiteSpace(s);
    }

    public static bool IsNotNullOrWhiteSpace(this string? s)
    {
        return !s.IsNullOrWhiteSpace();
    }

    public static string GetNullTerminatedString(this byte[] arr, int sliceEnd = -1)
    {
        var indexOfNullCharacter = Array.IndexOf(arr, byte.MinValue);
        if (indexOfNullCharacter < 0)
        {
            indexOfNullCharacter = sliceEnd >= 0 ? Math.Min(sliceEnd, arr.Length) : arr.Length;
        }

        return Encoding.ASCII.GetString(arr, 0, indexOfNullCharacter);
    }

    public static bool EndsWithAny(this string s, params char[] chars)
    {
        return chars.Any(c => s.EndsWith(c));
    }

    public static string EnsureStartsWith(this string? s, string? prefix, char separator = '\0')
    {
        var nonNullPrefix = prefix ?? "";
        var nonNullS = s ?? "";
        if (separator > 0)
        {
            nonNullS = nonNullS.TrimStart(separator);
            nonNullPrefix = nonNullPrefix.TrimEnd(separator);
        }

        if (Uri.IsWellFormedUriString(nonNullS, UriKind.Absolute))
            return nonNullS;

        if (separator > 0)
            return string.Join(separator, nonNullPrefix, nonNullS);

        return $"{nonNullPrefix}{nonNullS}";
    }
}