using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using IconPacks.Avalonia.RemixIcon;

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

    public object? ToolTipContent
    {
        get => GetValue(ToolTipContentProperty);
        set => SetValue(ToolTipContentProperty, value);
    }

    public PackIconRemixIconKind Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    public static readonly StyledProperty<string> HeaderProperty =
        AvaloniaProperty.Register<Infotip, string>(nameof(Header), "", false, BindingMode.TwoWay);

    public static readonly StyledProperty<object?> ToolTipContentProperty =
        AvaloniaProperty.Register<Infotip, object?>(nameof(ToolTipContent), null, false, BindingMode.TwoWay);

    public static readonly StyledProperty<PackIconRemixIconKind> IconProperty =
        AvaloniaProperty.Register<Infotip, PackIconRemixIconKind>(nameof(Icon), PackIconRemixIconKind.QuestionLine, false,
            BindingMode.TwoWay);
}