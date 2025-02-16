using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace TombLauncher.Controls;

public partial class LabeledTextBox : UserControl
{
    public LabeledTextBox()
    {
        InitializeComponent();
    }

    public static readonly StyledProperty<string> LabelProperty = AvaloniaProperty.Register<LabeledTextBox, string>(
        nameof(Label));

    public string Label
    {
        get => GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    public static readonly StyledProperty<string> TextProperty = AvaloniaProperty.Register<LabeledTextBox, string>(
        nameof(Text));

    public string Text
    {
        get => GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public static readonly StyledProperty<Infotip> InfotipProperty = AvaloniaProperty.Register<LabeledTextBox, Infotip>(
        nameof(Infotip));

    public Infotip Infotip
    {
        get => GetValue(InfotipProperty);
        set => SetValue(InfotipProperty, value);
    }
}