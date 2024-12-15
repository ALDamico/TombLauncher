using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Markup.Xaml;
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

    public static readonly StyledProperty<string> HeaderProperty =
        AvaloniaProperty.Register<Infotip, string>(nameof(Header), default, false, BindingMode.TwoWay);

    public static readonly StyledProperty<object> ToolTipContentProperty =
        AvaloniaProperty.Register<Infotip, object>(nameof(ToolTipContent), default, false, BindingMode.TwoWay);
}