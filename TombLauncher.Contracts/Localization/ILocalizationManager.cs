using System.Globalization;

namespace TombLauncher.Contracts.Localization;

public interface ILocalizationManager
{
    CultureInfo CurrentCulture { get; }
    string DateOnlyFormat { get; }
    string DateTimeFormat { get; }
    string GetLanguagesFolder();
    List<CultureInfo> GetSupportedLanguages();
    void ChangeLanguage(CultureInfo targetLanguage);
    Dictionary<string, string> GetSubsetInvertedByPrefix(string prefix);
    string GetLocalizedString(string key, params object[] parms);
    string this[string key] { get; }
}