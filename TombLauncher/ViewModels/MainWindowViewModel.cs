using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using Material.Icons;
using TombLauncher.Core.Navigation;
using TombLauncher.Core.PlatformSpecific;
using TombLauncher.Localization.Extensions;
using TombLauncher.Services;
using TombLauncher.ViewModels.Pages;

namespace TombLauncher.ViewModels;

public partial class MainWindowViewModel : WindowViewModelBase
{
    public MainWindowViewModel(NavigationManager navigationManager, NotificationListViewModel notificationListViewModel)
    {
        _navigationManager = navigationManager;
        _navigationManager.PropertyChanged += NavigationManagerOnPropertyChanged;
        NotificationListViewModel = notificationListViewModel;
        TogglePaneCmd = new RelayCommand(TogglePane);
        GoBackCmd = new RelayCommand(GoBack, CanGoBack);
        OpenSettingsCmd = new AsyncRelayCommand(OpenSettings);
        MenuItems = new ObservableCollection<MainMenuItemViewModel>()
        {
            new MainMenuItemViewModel()
            {
                ToolTip = "Welcome".GetLocalizedString(),
                Icon = MaterialIconKind.HomeOutline,
                Text = "Welcome".GetLocalizedString(),
                ViewModelType = typeof(WelcomePageViewModel)
            },
            new MainMenuItemViewModel()
            {
                ToolTip = "My mods".GetLocalizedString(),
                Icon = MaterialIconKind.Games,
                Text = "My mods".GetLocalizedString(),
                ViewModelType = typeof(GameListViewModel)
            },
            new MainMenuItemViewModel()
            {
                ToolTip = "Search".GetLocalizedString(),
                Icon = MaterialIconKind.Magnify,
                Text = "Search".GetLocalizedString(),
                ViewModelType = typeof(GameSearchViewModel)
            },
            new MainMenuItemViewModel()
            {
                ToolTip = "Random".GetLocalizedString(),
                Icon = MaterialIconKind.Gambling,
                Text = "Random game".GetLocalizedString(),
                ViewModelType = typeof(RandomGameViewModel)
            },
            new MainMenuItemViewModel()
            {
                ToolTip = "Statistics".GetLocalizedString(),
                Icon = MaterialIconKind.ChartBar,
                Text = "Statistics".GetLocalizedString(),
                ViewModelType = typeof(StatisticsPageViewModel)
            }
        };

        SettingsItem = new MainMenuItemViewModel()
        {
            ToolTip = "Settings".GetLocalizedString(),
            Icon = MaterialIconKind.Settings,
            Text = "Settings".GetLocalizedString(),
            ViewModelType = typeof(SettingsPageViewModel)
        };

        GitHubLinkItem = new CommandViewModel()
        {
            Tooltip = "Open Tomb Launcher's GitHub page".GetLocalizedString(),
            Icon = MaterialIconKind.Github,
            Text = "GitHub",
            Command = new RelayCommand(OpenGithub)
        };

        Title = "Tomb Launcher";
        // Initialize default view
        SelectedMenuItem = MenuItems.First();
    }

    private void NavigationManagerOnPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(NavigationManager.CurrentPage))
        {
            OnPropertyChanged(nameof(CurrentPage));
        }

        if (e.PropertyName == nameof(NavigationManager.CanGoBack))
        {
            Dispatcher.UIThread.Invoke(() => ((RelayCommand)GoBackCmd).NotifyCanExecuteChanged());
        }
    }

    private readonly NavigationManager _navigationManager;
    [ObservableProperty] private NotificationListViewModel _notificationListViewModel;
    [ObservableProperty] private MainMenuItemViewModel _settingsItem;
    [ObservableProperty] private CommandViewModel _gitHubLinkItem;
    [ObservableProperty] private bool _isSettingsOpen;

    private void OpenGithub()
    {
        var settings = Ioc.Default.GetRequiredService<SettingsService>();
        var gitHubLink = settings.GetGitHubLink();
        var platformSpecificFeatures = Ioc.Default.GetRequiredService<IPlatformSpecificFeatures>();
        platformSpecificFeatures.OpenUrl(gitHubLink);
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
            if (value != SettingsItem)
                IsSettingsOpen = false;
            SetProperty(ref _selectedMenuItem, value);
            if (value != null)
            {
                // Using NavigateToRoot for main menu items to clear history stack
                _navigationManager.NavigateToRoot(value.ViewModelType);
            }
        }
    }


    public INavigationTarget CurrentPage => _navigationManager.CurrentPage as INavigationTarget;
    public ICommand GoBackCmd { get; }

    private async void GoBack()
    {
        await _navigationManager.GoBack();
    }

    private bool CanGoBack() => _navigationManager.CanGoBack;
    public ICommand OpenSettingsCmd { get; }

    private async Task OpenSettings()
    {
        await _navigationManager.NavigateTo(SettingsItem.ViewModelType);
        SelectedMenuItem = SettingsItem;
        IsSettingsOpen = true;
    }
}