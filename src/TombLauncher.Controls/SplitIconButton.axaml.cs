using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using IconPacks.Avalonia.RemixIcon;

namespace TombLauncher.Controls;

public partial class SplitIconButton : SplitButton
{
    public SplitIconButton()
    {
        InitializeComponent();
    }

    protected override Type StyleKeyOverride => typeof(SplitButton);

    public static readonly StyledProperty<PackIconRemixIconKind> IconProperty =
        AvaloniaProperty.Register<SplitIconButton, PackIconRemixIconKind>(nameof(Icon), default, false, BindingMode.OneTime);

    public PackIconRemixIconKind Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    public static readonly StyledProperty<string> TextProperty =
        AvaloniaProperty.Register<SplitIconButton, string>(nameof(Text), "", false, BindingMode.TwoWay);

    public string Text
    {
        get => GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public static readonly StyledProperty<bool> IsTextVisibleProperty =
        AvaloniaProperty.Register<SplitIconButton, bool>(nameof(IsTextVisible), true);

    public bool IsTextVisible
    {
        get => GetValue(IsTextVisibleProperty);
        set => SetValue(IsTextVisibleProperty, value);
    }
}