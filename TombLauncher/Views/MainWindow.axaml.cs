using Avalonia.Controls;
using Avalonia.Interactivity;
using TombLauncher.ViewModels;

namespace TombLauncher.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private void Button_OnClick(object sender, RoutedEventArgs e)
    {
        (DataContext as MainWindowViewModel).NotificationListViewModel.HasNewItems = false;
    }
}