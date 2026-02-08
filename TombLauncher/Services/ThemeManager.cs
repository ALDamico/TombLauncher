using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml.Styling;
using Avalonia.Styling;
using TombLauncher.Contracts.Enums;

namespace TombLauncher.Services;

public class ThemeManager
{
    private readonly ResourceDictionary _scionTheme;
    private readonly ResourceDictionary _xianTheme;

    public ThemeManager()
    {
        // Load themes from assets
        // Since we created them as ResourceDictionaries in Assets/Themes/, we can load them via ResourceInclude logic 
        // or just constructing them if we had code behind. 
        // For XAML-only, we usually use styles. But here they are ResourceDictionaries.
        // We will simple create StyleIncludes or ResourceIncludes dynamically.

        // Actually, the cleanest way in Avalonia is to use Styles for themes if they contain resources.
        // But let's stick to ResourceDictionary management in App.Resources.MergedDictionaries.
    }

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
