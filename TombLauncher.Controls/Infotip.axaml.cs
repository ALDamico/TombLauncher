using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Material.Icons;
using Material.Icons.Avalonia;

namespace TombLauncher.Controls;

public partial class Infotip : UserControl
{
    public Infotip()
    {
        InitializeComponent();
    }

    public string Header
    {
        get => GetValue(HeaderProperty);
        set => SetValue(HeaderProperty, value);
    }

    public object ToolTipContent
    {
        get => GetValue(ToolTipContentProperty);
        set => SetValue(ToolTipContentProperty, value);
    }

    public MaterialIconKind Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    public static readonly StyledProperty<string> HeaderProperty =
        AvaloniaProperty.Register<Infotip, string>(nameof(Header), default, false, BindingMode.TwoWay);

    public static readonly StyledProperty<object> ToolTipContentProperty =
        AvaloniaProperty.Register<Infotip, object>(nameof(ToolTipContent), default, false, BindingMode.TwoWay);

    public static readonly StyledProperty<MaterialIconKind> IconProperty =
        AvaloniaProperty.Register<Infotip, MaterialIconKind>(nameof(Icon), MaterialIconKind.QuestionMarkCircle, false,
            BindingMode.TwoWay);

    public static readonly StyledProperty<IBrush> IconForegroundProperty = AvaloniaProperty.Register<Infotip, IBrush>(
        nameof(IconForeground), default, false, BindingMode.Default);

    public IBrush IconForeground
    {
        get => GetValue(IconForegroundProperty);
        set => SetValue(IconForegroundProperty, value);
    }
}