using System.Globalization;

namespace TombLauncher.Contracts.Localization.Dtos;

public class AvailableLanguageDto
{
    public required string CountryIso2Code { get; set; }
    public required string DictionaryName { get; set; }
    public required string DisplayName { get; set; }
    public required CultureInfo Culture { get; set; }
}