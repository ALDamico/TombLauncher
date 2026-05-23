using System;
using CommunityToolkit.Mvvm.ComponentModel;
using IconPacks.Avalonia.RemixIcon;

namespace TombLauncher.ViewModels;

public partial class MainMenuItemViewModel : ViewModelBase
{
    [ObservableProperty]
    public partial PackIconRemixIconKind Icon { get; set; }

    [ObservableProperty]
    public partial string Text { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string ToolTip { get; set; } = string.Empty;

    [ObservableProperty]
    public partial Type? ViewModelType { get; set; }
}