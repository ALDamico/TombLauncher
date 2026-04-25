using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using IconPacks.Avalonia.RemixIcon;

namespace TombLauncher.Controls;

public partial class RoundIconButton : Button
{
    public RoundIconButton()
    {
        InitializeComponent();
    }

    protected override Type StyleKeyOverride => typeof(Button);

    public static readonly StyledProperty<PackIconRemixIconKind> IconProperty =
        AvaloniaProperty.Register<RoundIconButton, PackIconRemixIconKind>(nameof(Icon), default, false, BindingMode.OneTime);

    public PackIconRemixIconKind Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }
}