using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using Material.Icons;

namespace TombLauncher.ViewModels;

public partial class MainMenuItemViewModel : ViewModelBase
{
    [ObservableProperty] private MaterialIconKind _icon;

    [ObservableProperty] private string _text;

    [ObservableProperty] private string _toolTip;

    [ObservableProperty] private Func<Task<PageViewModel>> _pageViewModelFactory;
}