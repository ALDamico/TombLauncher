using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using Material.Icons;

namespace TombLauncher.Controls;

public partial class IconButton : Button
{
    public IconButton()
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
    /*
    public static readonly new StyledProperty<ICommand?> CommandProperty =
        AvaloniaProperty.Register<Button, ICommand?>(nameof(Command), enableDataValidation: true);
    
    public new ICommand? Command
    {
        get => GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }
    */
}