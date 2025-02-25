using Avalonia.Controls;
using Avalonia.Input;
using TombLauncher.ViewModels;
using GameSearchViewModel = TombLauncher.ViewModels.Pages.GameSearchViewModel;

namespace TombLauncher.Views.Pages;

public partial class GameSearchView : UserControl
{
    public GameSearchView()
    {
        InitializeComponent();
    }

    private void InputElement_OnKeyUp(object sender, KeyEventArgs e)
    {
        if (DataContext is not GameSearchViewModel dataContext) return;
        if (e.Key == Key.Enter)
        {
            dataContext.SearchCmd.Execute(null);
        }
    }
}