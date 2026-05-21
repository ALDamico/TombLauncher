using System;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using TombLauncher.Contracts.Navigation;

namespace TombLauncher.ViewModels;

public partial class CommandViewModel : ViewModelBase, ITopBarCommand
{
    [ObservableProperty]
    public partial ICommand Command { get; set; } = null!;

    [ObservableProperty]
    public partial Enum? Icon { get; set; }

    [ObservableProperty]
    public partial string Tooltip { get; set; } = null!;

    [ObservableProperty]
    public partial string Text { get; set; } = null!;
}