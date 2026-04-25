using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml.Styling;
using Avalonia.Styling;
using TombLauncher.Contracts.Enums;

namespace TombLauncher.Services;

public class ThemeManager
{
    public void ApplyTheme(string themeName)
    {
        var app = Application.Current;
        if (app == null) return;


        // Determine which file to load
        string xamlFile = themeName switch
        {
            "Xian" => "avares://TombLauncher/Assets/Themes/XianTheme.axaml",
            "Xian Light" => "avares://TombLauncher/Assets/Themes/XianLight.axaml",
            "Scion" => "avares://TombLauncher/Assets/Themes/ScionTheme.axaml",
            "Scion Light" => "avares://TombLauncher/Assets/Themes/ScionLight.axaml",
            "Horus" => "avares://TombLauncher/Assets/Themes/HorusTheme.axaml",
            "Horus Light" => "avares://TombLauncher/Assets/Themes/HorusLight.axaml",
            _ => "avares://TombLauncher/Assets/Themes/ScionTheme.axaml" // Default
        };

        // Load the new dictionary
        var newTheme = (ResourceDictionary)Avalonia.Markup.Xaml.AvaloniaXamlLoader.Load(new Uri(xamlFile));



        // Find and replace the existing theme dictionary
        // We assume the theme dictionary is the first one in MergedDictionaries based on App.axaml structure
        if (app.Resources.MergedDictionaries.Count > 0)
        {
            // We expect the theme to be at index 0 because we added it first in App.axaml
            app.Resources.MergedDictionaries[0] = newTheme;
        }
        else
        {
            app.Resources.MergedDictionaries.Add(newTheme);
        }
    }
}
