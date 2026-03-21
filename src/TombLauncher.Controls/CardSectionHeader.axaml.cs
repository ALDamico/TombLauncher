using Avalonia;
using Avalonia.Controls;
using IconPacks.Avalonia.RemixIcon;

namespace TombLauncher.Controls;

public partial class CardSectionHeader : UserControl
{
    public static readonly StyledProperty<PackIconRemixIconKind> IconKindProperty =
        AvaloniaProperty.Register<CardSectionHeader, PackIconRemixIconKind>(nameof(IconKind));

    public static readonly StyledProperty<string> TitleProperty =
        AvaloniaProperty.Register<CardSectionHeader, string>(nameof(Title), defaultValue: string.Empty);

    public PackIconRemixIconKind IconKind
    {
        get => GetValue(IconKindProperty);
        set => SetValue(IconKindProperty, value);
    }

    public string Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public CardSectionHeader()
    {
        InitializeComponent();
    }
}
