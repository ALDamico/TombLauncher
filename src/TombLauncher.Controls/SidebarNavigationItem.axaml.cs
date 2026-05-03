using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using IconPacks.Avalonia.RemixIcon;

namespace TombLauncher.Controls;

public partial class SidebarNavigationItem : ListBoxItem
{
    public SidebarNavigationItem()
    {
        InitializeComponent();
    }

    protected override Type StyleKeyOverride => typeof(ListBoxItem);

    public static readonly StyledProperty<ICommand?> CommandProperty =
        AvaloniaProperty.Register<SidebarNavigationItem, ICommand?>(nameof(Command));

    public ICommand? Command
    {
        get => GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }

    public static readonly StyledProperty<PackIconRemixIconKind> IconProperty =
        AvaloniaProperty.Register<SidebarNavigationItem, PackIconRemixIconKind>(nameof(Icon));

    public PackIconRemixIconKind Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    public static readonly StyledProperty<string> TextProperty =
        AvaloniaProperty.Register<SidebarNavigationItem, string>(nameof(Text), string.Empty);

    public string Text
    {
        get => GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);
        if (e.InitialPressMouseButton == MouseButton.Left)
            Command?.Execute(null);
    }
}
