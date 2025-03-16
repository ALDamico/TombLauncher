using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Material.Icons;
using Material.Icons.Avalonia;

namespace TombLauncher.Views;

public partial class RadioButtonDetailView : UserControl
{
    public RadioButtonDetailView()
    {
        InitializeComponent();
    }

    public static readonly StyledProperty<MaterialIconKind> IconProperty = AvaloniaProperty.Register<RadioButtonDetailView, MaterialIconKind>(
        nameof(Icon));

    public MaterialIconKind Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }
    
    public static readonly StyledProperty<string> TextProperty = AvaloniaProperty.Register<RadioButtonDetailView, string>(
        nameof(Text));

    public string Text
    {
        get => GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public static readonly StyledProperty<string> DetailsLine1Property = AvaloniaProperty.Register<RadioButtonDetailView, string>(
        nameof(DetailsLine1));

    public string DetailsLine1
    {
        get => GetValue(DetailsLine1Property);
        set => SetValue(DetailsLine1Property, value);
    }

    public static readonly StyledProperty<string> DetailsLine2Property = AvaloniaProperty.Register<RadioButtonDetailView, string>(
        nameof(DetailsLine2));

    public string DetailsLine2
    {
        get => GetValue(DetailsLine2Property);
        set => SetValue(DetailsLine2Property, value);
    }
}