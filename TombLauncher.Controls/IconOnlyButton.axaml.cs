using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Markup.Xaml;
using Material.Icons;

namespace TombLauncher.Controls;

public partial class IconOnlyButton : Button
{
    public IconOnlyButton()
    {
        InitializeComponent();
    }
    
    protected override Type StyleKeyOverride => typeof(Button);

    public static readonly StyledProperty<MaterialIconKind> IconProperty =
        AvaloniaProperty.Register<IconButton, MaterialIconKind>(nameof(Icon), default, false, BindingMode.OneTime);

    public MaterialIconKind Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }
}