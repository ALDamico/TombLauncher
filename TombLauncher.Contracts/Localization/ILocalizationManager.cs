using System.Globalization;
using TombLauncher.Contracts.Localization.Dtos;

namespace TombLauncher.Contracts.Localization;

public interface ILocalizationManager
{
    CultureInfo CurrentCulture { get; }
    string DateOnlyFormat { get; }
    string DateTimeFormat { get; }
    string GetLanguagesFolder();
    List<AvailableLanguageDto> GetSupportedLanguages();
    void ChangeLanguage(CultureInfo targetLanguage);
    Dictionary<string, string> GetSubsetInvertedByPrefix(string prefix);
    string GetLocalizedString(string key, params object[] parms);
    string this[string key] { get; }
}