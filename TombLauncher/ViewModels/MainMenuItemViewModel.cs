using System;
using CommunityToolkit.Mvvm.ComponentModel;
using Material.Icons;

namespace TombLauncher.ViewModels;

public partial class MainMenuItemViewModel : ViewModelBase
{
    [ObservableProperty] private MaterialIconKind _icon;

    [ObservableProperty] private string _text;

    [ObservableProperty] private string _toolTip;

    private Lazy<PageViewModel> _pageViewModelFactory;

    public PageViewModel PageViewModelFactory
    {
        get => _pageViewModelFactory.Value;
        set => _pageViewModelFactory = new Lazy<PageViewModel>(() => value);
    }
}