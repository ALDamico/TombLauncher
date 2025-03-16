using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using Material.Icons;
using TombLauncher.Core.Navigation;

namespace TombLauncher.ViewModels;

public partial class CommandViewModel : ViewModelBase, ITopBarCommand
{
    [ObservableProperty] private ICommand _command;
    [ObservableProperty] private MaterialIconKind _icon;
    [ObservableProperty] private string _tooltip;
    [ObservableProperty] private string _text;
}