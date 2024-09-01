using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
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

    public CultureInfo CurrentCulture => _currentCulture;

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

        var resourceKey =
            $"{Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}/{_localizationRelativePath}/{cultureName}.axaml";

        var resourceKeyDefault =
            $"{Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}/{_localizationRelativePath}/{_defaultCulture}.axaml";
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
        var missingKeys = rdDefault.Keys.Except(rd.Keys);
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
                element.Key.ToString().Remove(prefix).ToLowerInvariant());
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
}