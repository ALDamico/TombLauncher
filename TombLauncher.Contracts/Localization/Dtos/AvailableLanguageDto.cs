using System.Globalization;

namespace TombLauncher.Contracts.Localization.Dtos;

public class AvailableLanguageDto
{
    public string CountryIso2Code { get; set; }
    public string DictionaryName { get; set; }
    public string DisplayName { get; set; }
    public CultureInfo Culture { get; set; }
}