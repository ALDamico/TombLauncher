using System.Globalization;
using System.Reflection;
using System.Text.RegularExpressions;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Markup.Xaml.Styling;
using TombLauncher.Contracts.Localization;
using TombLauncher.Contracts.Localization.Dtos;
using TombLauncher.Contracts.Settings;

namespace TombLauncher.Localization;

public class LocalizationManager : ILocalizationManager, ISettingsVisitable
{
    public LocalizationManager(Application application)
    {
        _currentCulture = CultureInfo.CurrentUICulture;
        _defaultCulture = CultureInfo.GetCultureInfo("en-US");
        _application = application;
        _localizationRelativePath = "Localization";
        ChangeLanguage(_defaultCulture);
    }

    private CultureInfo _currentCulture;
    private string _localizationRelativePath;
    private CultureInfo _defaultCulture;
    private Application _application;
    private ResourceDictionary _localizedStrings;

    public CultureInfo CurrentCulture => _currentCulture;

    public string GetLanguagesFolder()
    {
        return $"{Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}/{_localizationRelativePath}";
    }

    public List<AvailableLanguageDto> GetSupportedLanguages()
    {
        var cultureInfos = new List<AvailableLanguageDto>();
        var languagesFolder = GetLanguagesFolder();
        var dictionaryFiles = Directory.GetFiles(languagesFolder, "*.axaml");
        foreach (var file in dictionaryFiles)
        {
            try
            {
                var fileName = Path.GetFileNameWithoutExtension(file);
                var cultureInfo = CultureInfo.GetCultureInfo(fileName);

                var countryCode = Regex.Match(fileName, @"-(\w{2})").Groups[1].Value;
                var dto = new AvailableLanguageDto()
                {
                    Culture = cultureInfo,
                    DictionaryName = fileName,
                    DisplayName = cultureInfo.DisplayName,
                    CountryIso2Code = countryCode
                };
                cultureInfos.Add(dto);
            }
            catch (CultureNotFoundException)
            {
                // Ignored for now
            }
        }

        return cultureInfos;
    }

    public void ChangeLanguage(CultureInfo targetLanguage)
    {
        var currentApp = Application.Current;
        if (currentApp == null) return;
        _currentCulture = targetLanguage;
        var cultureName = _currentCulture.Name;
        var currentTranslations = currentApp.Resources.MergedDictionaries.OfType<ResourceInclude>()
            .FirstOrDefault(dic => dic.Source?.OriginalString?.Contains(_localizationRelativePath) ?? false);
        CultureInfo.CurrentUICulture = _currentCulture;
        if (currentTranslations != null)
        {
            _application.Resources.MergedDictionaries.Remove(currentTranslations);
        }

        var resourceKey =
            $"{GetLanguagesFolder()}/{cultureName}.axaml";

        var resourceKeyDefault =
            $"{GetLanguagesFolder()}/{_defaultCulture}.axaml";
        var defaultKeyXaml = File.ReadAllText(resourceKeyDefault);
        string xaml;

        try
        {
            xaml = File.ReadAllText(
                resourceKey);
        }
        catch (FileNotFoundException)
        {
            xaml = defaultKeyXaml;
        }

        var rd = AvaloniaRuntimeXamlLoader.Parse<ResourceDictionary>(xaml);
        var rdDefault = AvaloniaRuntimeXamlLoader.Parse<ResourceDictionary>(defaultKeyXaml);
        var missingKeys = Enumerable.Except<object>(rdDefault.Keys, rd.Keys);
        var resultingDictionary = new ResourceDictionary();
        foreach (var key in missingKeys)
        {
            resultingDictionary.Add(key, rdDefault[key]);
        }

        foreach (var key in rd.Keys)
        {
            resultingDictionary.TryAdd(key, rd[key]);
        }

        _application.Resources.MergedDictionaries.Add(resultingDictionary);
        _localizedStrings = resultingDictionary;
    }

    public Dictionary<string, string> GetSubsetInvertedByPrefix(string prefix)
    {
        var elements = _localizedStrings.Where(ls => (ls.Key as string)?.StartsWith(prefix + "_") == true);
        var dictionary = new Dictionary<string, string>();
        foreach (var element in elements)
        {
            dictionary.TryAdd(element.Value.ToString().ToLowerInvariant(),
                element.Key.ToString().Replace(prefix, string.Empty).ToLowerInvariant());
        }

        return dictionary;
    }

    public string GetLocalizedString(string key, params object[] parms)
    {
        if (parms == null)
        {
            parms = [];
        }

        return !_localizedStrings.TryGetValue(key, out var s) ? key : string.Format((string)s ?? key, parms);
    }

    public string this[string key] => GetLocalizedString(key);
    public string DateOnlyFormat => GetLocalizedString(nameof(DateOnlyFormat));
    public string DateTimeFormat => GetLocalizedString(nameof(DateTimeFormat));

    public void Accept(ISettingsVisitor visitor)
    {
        visitor.Visit(this);
    }
}