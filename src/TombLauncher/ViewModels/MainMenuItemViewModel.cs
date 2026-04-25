using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using IconPacks.Avalonia.RemixIcon;
using TombLauncher.Core.Navigation;

namespace TombLauncher.ViewModels;

public partial class MainMenuItemViewModel : ViewModelBase
{
    [ObservableProperty] private PackIconRemixIconKind _icon;

    [ObservableProperty] private string _text = string.Empty;

    [ObservableProperty] private string _toolTip = string.Empty;

    [ObservableProperty] private Type? _viewModelType;
}