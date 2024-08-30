using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Markup.Xaml.Styling;
using TombLauncher.Extensions;

namespace TombLauncher.Localization;

public class LocalizationManager
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

    public void ChangeLanguage(CultureInfo targetLanguage)
    {
        _currentCulture = targetLanguage;
        var cultureName = _currentCulture.Name;
        var currentTranslations = App.Current.Resources.MergedDictionaries.OfType<ResourceInclude>()
            .FirstOrDefault(dic => dic.Source?.OriginalString?.Contains(_localizationRelativePath) ?? false);
        if (currentTranslations != null)
        {
            _application.Resources.MergedDictionaries.Remove(currentTranslations);
        }

        var resourceKey = $"avares://{_application.Name}/{_localizationRelativePath}/{cultureName}.axaml";
        var resourceKeyDefault = $"avares://{_application.Name}/{_localizationRelativePath}/{_defaultCulture.Name}.axaml";
        var rd = (ResourceDictionary)AvaloniaXamlLoader.Load(new Uri(resourceKey));
        var rdDefault = (ResourceDictionary)AvaloniaXamlLoader.Load(new Uri(resourceKeyDefault));
        var missingKeys = rdDefault.Keys.Except(rd.Keys);
        var resultingDictionary = new ResourceDictionary();
        foreach (var key in missingKeys)
        {
            resultingDictionary.Add(key, rdDefault[key]);
        }

        foreach (var key in rd.Keys)
        {
            resultingDictionary.Add(key, rd[key]);
        }
        
        _application.Resources.MergedDictionaries.Add(resultingDictionary);
        _localizedStrings = resultingDictionary;
    }

    public Dictionary<string, string> GetSubsetInvertedByPrefix(string prefix)
    {
        return _localizedStrings.Where(ls => (ls.Key as string)?.StartsWith(prefix + "_") == true)
            .ToDictionary(pair => pair.Value.ToString().ToLowerInvariant(), pair => pair.Key.ToString().Remove(prefix).ToLowerInvariant());
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
}