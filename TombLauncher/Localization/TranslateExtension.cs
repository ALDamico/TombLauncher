using System;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Markup.Xaml;

namespace TombLauncher.Localization;

public class TranslateExtension : MarkupExtension
{
    public TranslateExtension(string key)
    {
        Key = key;
    }

    public string Key { get; }

    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        var localizationManager = LocalizationManager.Instance;

        if (localizationManager == null)
        {
            // Fallback for design time or early startup.
            return Key;
        }

        // Return a binding to the LocalizationManager indexer.
        // Because LocalizationManager implements INotifyPropertyChanged and raises for Binding.IndexerName,
        // this binding will automatically update when the language changes.
        var binding = new Binding($"[{Key}]")
        {
            Source = localizationManager,
            Mode = BindingMode.OneWay
        };

        return binding;
    }
}
