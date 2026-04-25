using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.VisualTree;

namespace TombLauncher.Views;

public partial class GameSearchResultCardView : UserControl
{
    public static readonly StyledProperty<ICommand?> TapCommandProperty =
        AvaloniaProperty.Register<GameSearchResultCardView, ICommand?>(nameof(TapCommand));

    public ICommand? TapCommand
    {
        get => GetValue(TapCommandProperty);
        set => SetValue(TapCommandProperty, value);
    }

    public GameSearchResultCardView()
    {
        InitializeComponent();
    }

    private void OnCardTapped(object? sender, TappedEventArgs e)
    {
        // Don't navigate if a button was the target
        if (e.Source is Visual visual && visual.FindAncestorOfType<Button>(includeSelf: true) != null)
            return;

        if (TapCommand != null && TapCommand.CanExecute(DataContext))
        {
            TapCommand.Execute(DataContext);
        }
    }
}