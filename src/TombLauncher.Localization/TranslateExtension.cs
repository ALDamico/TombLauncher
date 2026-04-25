using Avalonia;
using Avalonia.Data;
using Avalonia.Markup.Xaml;
using TombLauncher.Localization.Converters;

namespace TombLauncher.Localization;

public class TranslateExtension : MarkupExtension
{
    public TranslateExtension(string key)
    {
        Key = key;
    }

    public string Key { get; }
    public StringCasing Casing { get; set; } = StringCasing.Normal;

    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        var localizationManager = LocalizationManager.Instance;

        if (localizationManager == null)
        {
            // Fallback for design time or early startup.
            return Key;
        }

        // Check if the target property supports bindings (i.e. is an AvaloniaProperty).
        // If not (e.g. a plain CLR string property like BooleanToStringConverter.TrueValue),
        // return the resolved string directly to avoid InvalidCastException.
        var provideValueTarget = serviceProvider.GetService(typeof(IProvideValueTarget)) as IProvideValueTarget;
        if (provideValueTarget?.TargetProperty is not AvaloniaProperty)
        {
            var value = localizationManager[Key];
            return ApplyCasing(value);
        }

        // Return a binding to the LocalizationManager indexer.
        // Because LocalizationManager implements INotifyPropertyChanged and raises for Binding.IndexerName,
        // this binding will automatically update when the language changes.
        var binding = new Binding($"[{Key}]")
        {
            Source = localizationManager,
            Mode = BindingMode.OneWay,
            Converter = new StringCasingConverter { Casing = Casing }
        };

        return binding;
    }

    private string ApplyCasing(string value)
    {
        return Casing switch
        {
            StringCasing.Upper => value.ToUpperInvariant(),
            StringCasing.Lower => value.ToLowerInvariant(),
            _ => value
        };
    }
}
