using Avalonia.Controls;
using Avalonia.Input;
using TombLauncher.ViewModels;

namespace TombLauncher.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.F11 && DataContext is MainWindowViewModel mainWindowVm)
        {
            mainWindowVm.ToggleFullScreenCommand.Execute(null);
            e.Handled = true;
        }
        base.OnKeyDown(e);
    }
}