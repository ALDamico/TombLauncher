using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Markup.Xaml;

namespace TombLauncher.Controls;

public partial class BadgedIconButton : IconButton
{
    public BadgedIconButton()
    {
        InitializeComponent();
    }

    public static readonly StyledProperty<bool> IsBadgeVisibleProperty =
        AvaloniaProperty.Register<BadgedIconButton, bool>(nameof(IsBadgeVisible), default, false, BindingMode.TwoWay);

    public bool IsBadgeVisible
    {
        get => GetValue(IsBadgeVisibleProperty);
        set => SetValue(IsBadgeVisibleProperty, value);
    }
}