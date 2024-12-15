using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using Material.Icons;
using TombLauncher.Localization.Extensions;
using TombLauncher.Navigation;
using TombLauncher.ViewModels.Pages;

namespace TombLauncher.ViewModels;

public partial class MainWindowViewModel : WindowViewModelBase
{
    public MainWindowViewModel(NavigationManager navigationManager)
    {
        _navigationManager = navigationManager;
        _navigationManager.OnNavigated += OnNavigated;
        TogglePaneCmd = new RelayCommand(TogglePane);
        GoBackCmd = new RelayCommand(GoBack, CanGoBack);
        MenuItems = new ObservableCollection<MainMenuItemViewModel>()
        {
            new MainMenuItemViewModel()
            {
                ToolTip = "Welcome".GetLocalizedString(),
                Icon = MaterialIconKind.HomeOutline,
                Text = "Welcome".GetLocalizedString(),
                PageViewModelFactory = Ioc.Default.GetRequiredService<WelcomePageViewModel>()
            },
            new MainMenuItemViewModel()
            {
                ToolTip = "My mods".GetLocalizedString(),
                Icon = MaterialIconKind.Games,
                Text = "My mods".GetLocalizedString(),
                PageViewModelFactory = Ioc.Default.GetRequiredService<GameListViewModel>()
            },
            new MainMenuItemViewModel()
            {
                ToolTip = "Search".GetLocalizedString(),
                Icon = MaterialIconKind.Magnify,
                Text = "Search".GetLocalizedString(),
                PageViewModelFactory = Ioc.Default.GetRequiredService<GameSearchViewModel>()
            },
            new MainMenuItemViewModel()
            {
                ToolTip = "Settings".GetLocalizedString(),
                Icon = MaterialIconKind.Settings,
                Text = "Settings".GetLocalizedString(),
                PageViewModelFactory = Ioc.Default.GetRequiredService<SettingsPageViewModel>()
            }
        };
        
        _navigationManager.StartNavigation(MenuItems.First().PageViewModelFactory);
        Title = "Tomb Launcher";
    }

    private void OnNavigated()
    {
        OnPropertyChanged(nameof(CurrentPage));
        Dispatcher.UIThread.Invoke(() => ((RelayCommand)GoBackCmd).NotifyCanExecuteChanged());
    }

    private readonly NavigationManager _navigationManager;

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
                _navigationManager.StartNavigation(value.PageViewModelFactory);
                OnNavigated();
            }
        }
    }

    public PageViewModel CurrentPage => _navigationManager.GetCurrentPage();
    public ICommand GoBackCmd { get; }

    private void GoBack()
    {
        _navigationManager.GoBack();
        OnNavigated();
    }

    private bool CanGoBack() => _navigationManager.CanGoBack();
    
    
}