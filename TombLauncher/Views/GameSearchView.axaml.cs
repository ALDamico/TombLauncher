using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using TombLauncher.ViewModels;

namespace TombLauncher.Views;

public partial class GameSearchView : UserControl
{
    public GameSearchView()
    {
        InitializeComponent();
    }

    private void InputElement_OnKeyUp(object sender, KeyEventArgs e)
    {
        var dataContext = this.DataContext as GameSearchViewModel;
        if (dataContext == null) return;
        if (e.Key == Key.Enter)
        {
            dataContext.SearchCmd.Execute(null);
        }
    }
}