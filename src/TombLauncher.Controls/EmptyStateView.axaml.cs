using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using IconPacks.Avalonia.RemixIcon;

namespace TombLauncher.Controls;

public partial class EmptyStateView : UserControl
{
    public EmptyStateView()
    {
        InitializeComponent();
    }

    public static readonly StyledProperty<PackIconRemixIconKind> IconProperty =
        AvaloniaProperty.Register<EmptyStateView, PackIconRemixIconKind>(nameof(Icon), default, false, BindingMode.OneTime);

    public PackIconRemixIconKind Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    public static readonly StyledProperty<string> TitleProperty =
        AvaloniaProperty.Register<EmptyStateView, string>(nameof(Title), string.Empty);

    public string Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public static readonly StyledProperty<string> SubtitleProperty =
        AvaloniaProperty.Register<EmptyStateView, string>(nameof(Subtitle), string.Empty);

    public string Subtitle
    {
        get => GetValue(SubtitleProperty);
        set => SetValue(SubtitleProperty, value);
    }
}
