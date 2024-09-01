using System;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using CommunityToolkit.Mvvm.DependencyInjection;
using TombLauncher.Localization;

namespace TombLauncher.Extensions;

public static class StringExtensions
{
    public static string GetLocalizedString(this string s)
    {
        return s.GetLocalizedString(null);
    }

    public static string GetLocalizedString(this string s, params object[] args)
    {
        var localizationManager = Ioc.Default.GetRequiredService<LocalizationManager>();
        return localizationManager.GetLocalizedString(s, args);
    }
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
        var regex = new Regex(@"[\{\[\(].*[\}\]\)]");
        return regex.Replace(text, String.Empty);
    }
}