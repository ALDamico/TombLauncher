using System.Collections.ObjectModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using Material.Icons;
using TombLauncher.Database.UnitOfWork;
using TombLauncher.ViewModels.Navigation;

namespace TombLauncher.ViewModels.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    public MainWindowViewModel(GamesUnitOfWork gamesUoW, NavigationManager navigationManager)
    {
        _navigationManager = navigationManager;
        _navigationManager.OnNavigated += OnNavigated;
        TogglePaneCmd = new RelayCommand(TogglePane);
        GoBackCmd = new RelayCommand(GoBack, CanGoBack);
        MenuItems = new ObservableCollection<MainMenuItemViewModel>()
        {
            new MainMenuItemViewModel()
            {
                ToolTip = "Welcome",
                Icon = MaterialIconKind.HomeOutline,
                Text = "Welcome",
                PageViewModelFactory = Ioc.Default.GetRequiredService<WelcomePageViewModel>()
            },
            new MainMenuItemViewModel()
            {
                ToolTip = "My mods",
                Icon = MaterialIconKind.Games,
                Text = "My mods",
                PageViewModelFactory = Ioc.Default.GetRequiredService<GameListViewModel>()
            }
        };
        
        _navigationManager.StartNavigation(MenuItems.First().PageViewModelFactory);
        Title = "Tomb Launcher";
    }

    private void OnNavigated()
    {
        OnPropertyChanged(nameof(CurrentPage));
        ((RelayCommand)GoBackCmd).NotifyCanExecuteChanged();
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