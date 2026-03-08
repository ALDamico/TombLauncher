using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using IconPacks.Avalonia.RemixIcon;
using TombLauncher.Core.Navigation;

namespace TombLauncher.ViewModels;

public partial class CommandViewModel : ViewModelBase, ITopBarCommand
{
    [ObservableProperty] private ICommand _command = null!;
    [ObservableProperty] private PackIconRemixIconKind _icon;
    [ObservableProperty] private string _tooltip = null!;
    [ObservableProperty] private string _text = null!;
}