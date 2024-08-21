using System.Collections.ObjectModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using Material.Icons;
using TombLauncher.Database.Entities;
using TombLauncher.Database.UnitOfWork;
using TombLauncher.Models;

namespace TombLauncher.ViewModels.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    public MainWindowViewModel(GamesUnitOfWork gamesUoW)
    {
        TogglePaneCmd = new RelayCommand(TogglePane);
        MenuItems = new ObservableCollection<MainMenuItemViewModel>()
        {
            new MainMenuItemViewModel()
            {
                ToolTip = "Welcome",
                Icon = MaterialIconKind.HomeOutline,
                Text = "Welcome",
                PageViewModelFactory = new WelcomePageViewModel() { ChangeLogPath = "avares://TombLauncher/Data/CHANGELOG.md" }
            },
            new MainMenuItemViewModel()
            {
                ToolTip = "My mods",
                Icon = MaterialIconKind.Games,
                Text = "My mods",
                PageViewModelFactory = Ioc.Default.GetRequiredService<GameListViewModel>()
            }
        };
        CurrentPage = MenuItems.First().PageViewModelFactory;
        Title = "Tomb Launcher";
    }

    private bool _isPaneOpen;

    public bool IsPaneOpen
    {
        get => _isPaneOpen;
        set => SetProperty(ref _isPaneOpen, value);
    }

    public ICommand TogglePaneCmd { get; }

    private void TogglePane()
    {
        IsPaneOpen = !IsPaneOpen;
    }

    public ObservableCollection<MainMenuItemViewModel> MenuItems { get; }
    private MainMenuItemViewModel _selectedMenuItem;

    public MainMenuItemViewModel SelectedMenuItem
    {
        get => _selectedMenuItem;
        set
        {
            SetProperty(ref _selectedMenuItem, value);
            if (value != null)
            {
                CurrentPage = value.PageViewModelFactory;
            }
        }
    }

    [ObservableProperty] private PageViewModel _currentPage;
}