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
    
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsChecked))]
    public partial bool IsCheckable { get; set; }

    public bool IsChecked
    {
        get
        {
            if (!IsCheckable) return false;
            return field;
        }
        set => SetProperty(ref field, value);
    }
}