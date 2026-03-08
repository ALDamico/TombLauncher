using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using IconPacks.Avalonia.RemixIcon;

namespace TombLauncher.Controls;

public partial class IconOnlyButton : Button
{
    public IconOnlyButton()
    {
        InitializeComponent();
    }

    protected override Type StyleKeyOverride => typeof(Button);

    public static readonly StyledProperty<PackIconRemixIconKind> IconProperty =
        AvaloniaProperty.Register<IconOnlyButton, PackIconRemixIconKind>(nameof(Icon), default, false, BindingMode.OneTime);

    public PackIconRemixIconKind Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }
}