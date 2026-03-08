using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using IconPacks.Avalonia.RemixIcon;

namespace TombLauncher.Controls;

public partial class BadgedIconButton : Button
{
    public BadgedIconButton()
    {
        InitializeComponent();
    }

    protected override Type StyleKeyOverride => typeof(Button);

    public static readonly StyledProperty<bool> IsBadgeVisibleProperty =
        AvaloniaProperty.Register<BadgedIconButton, bool>(nameof(IsBadgeVisible), false, false, BindingMode.TwoWay);

    public bool IsBadgeVisible
    {
        get => GetValue(IsBadgeVisibleProperty);
        set => SetValue(IsBadgeVisibleProperty, value);
    }

    public static readonly StyledProperty<PackIconRemixIconKind> IconProperty =
        AvaloniaProperty.Register<BadgedIconButton, PackIconRemixIconKind>(nameof(Icon), default, false, BindingMode.OneTime);

    public PackIconRemixIconKind Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }
}