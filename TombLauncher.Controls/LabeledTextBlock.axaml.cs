using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace TombLauncher.Controls;

public partial class LabeledTextBlock : UserControl
{
    public LabeledTextBlock()
    {
        InitializeComponent();
    }

    public static readonly StyledProperty<string> LabelProperty = AvaloniaProperty.Register<LabeledTextBlock, string>(
        nameof(Label));

    public string Label
    {
        get => GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    public static readonly StyledProperty<string> TextProperty = AvaloniaProperty.Register<LabeledTextBlock, string>(
        nameof(Text));

    public string Text
    {
        get => GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }
}