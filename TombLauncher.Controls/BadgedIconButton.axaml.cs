using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Markup.Xaml;
using Material.Icons;

namespace TombLauncher.Controls;

public partial class BadgedIconButton : Button
{
    public BadgedIconButton()
    {
        InitializeComponent();
    }

    protected override Type StyleKeyOverride => typeof(Button);

    public static readonly StyledProperty<bool> IsBadgeVisibleProperty =
        AvaloniaProperty.Register<BadgedIconButton, bool>(nameof(IsBadgeVisible), default, false, BindingMode.TwoWay);

    public bool IsBadgeVisible
    {
        get => GetValue(IsBadgeVisibleProperty);
        set => SetValue(IsBadgeVisibleProperty, value);
    }
    
    public static readonly StyledProperty<MaterialIconKind> IconProperty =
        AvaloniaProperty.Register<IconButton, MaterialIconKind>(nameof(Icon), default, false, BindingMode.OneTime);

    public MaterialIconKind Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }
}