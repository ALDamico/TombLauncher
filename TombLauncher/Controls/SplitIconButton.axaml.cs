using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Markup.Xaml;
using Material.Icons;

namespace TombLauncher.Controls;

public partial class SplitIconButton : SplitButton 
{
    public SplitIconButton()
    {
        InitializeComponent();
    }
    
    protected override Type StyleKeyOverride => typeof(SplitButton);

    public static readonly StyledProperty<MaterialIconKind> IconProperty =
        AvaloniaProperty.Register<IconButton, MaterialIconKind>(nameof(Icon), default, false, BindingMode.OneTime);

    public MaterialIconKind Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    public static readonly StyledProperty<string> TextProperty =
        AvaloniaProperty.Register<IconButton, string>(nameof(Text), default, false, BindingMode.TwoWay);

    public string Text
    {
        get => GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public static readonly StyledProperty<bool> IsTextVisibleProperty =
        AvaloniaProperty.Register<IconButton, bool>(nameof(IsTextVisible), true);

    public bool IsTextVisible
    {
        get => GetValue(IsTextVisibleProperty);
        set => SetValue(IsTextVisibleProperty, value);
    }
}