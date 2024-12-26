using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using Material.Icons;

namespace TombLauncher.ViewModels;

public partial class CommandViewModel : ViewModelBase
{
    [ObservableProperty] private ICommand _command;
    [ObservableProperty] private MaterialIconKind _icon;
    [ObservableProperty] private string _tooltip;
}