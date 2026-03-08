using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.VisualTree;
using TombLauncher.ViewModels;
using TombLauncher.ViewModels.Pages;
using TombLauncher.Views.Pages;

namespace TombLauncher.Views;

public partial class GameSearchResultCardView : UserControl
{
    public GameSearchResultCardView()
    {
        InitializeComponent();
    }

    private void OnCardTapped(object? sender, TappedEventArgs e)
    {
        // Don't navigate if a button was the target
        if (e.Source is Visual visual && visual.FindAncestorOfType<Button>(includeSelf: true) != null)
            return;

        var gameSearchView = this.FindAncestorOfType<GameSearchView>();
        if (gameSearchView?.DataContext is GameSearchViewModel vm && DataContext is MultiSourceGameSearchResultMetadataViewModel card)
        {
            if (vm.OpenCmd.CanExecute(card))
            {
                vm.OpenCmd.Execute(card);
            }
        }
    }
}