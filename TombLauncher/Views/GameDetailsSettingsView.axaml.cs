using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using TombLauncher.ViewModels.Pages.Settings;

namespace TombLauncher.Views;

public partial class GameDetailsSettingsView : UserControl
{
    public GameDetailsSettingsView()
    {
        InitializeComponent();
    }

    private void InputElement_OnKeyUp(object sender, KeyEventArgs e)
    {
        if (DataContext is not GameDetailsSettingsViewModel dataContext) return;
        switch (e.Key)
        {
            case Key.Enter:
                dataContext.AddPatternCmd.Execute(dataContext.CurrentPattern);
                break;
            case Key.Escape:
                dataContext.ClearCurrentPatternCmd.Execute(null);
                break;
        }
    }
}