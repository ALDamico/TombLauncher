using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using Material.Icons;
using TombLauncher.Localization.Extensions;
using TombLauncher.Navigation;
using TombLauncher.Services;
using TombLauncher.Utils;
using TombLauncher.ViewModels.Pages;

namespace TombLauncher.ViewModels;

public partial class MainWindowViewModel : WindowViewModelBase
{
    public MainWindowViewModel(NavigationManager navigationManager, NotificationListViewModel notificationListViewModel)
    {
        _navigationManager = navigationManager;
        _navigationManager.OnNavigated += OnNavigated;
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
                PageViewModelFactory = async () =>
                    await Task.FromResult(Ioc.Default.GetRequiredService<WelcomePageViewModel>())
            },
            new MainMenuItemViewModel()
            {
                ToolTip = "My mods".GetLocalizedString(),
                Icon = MaterialIconKind.Games,
                Text = "My mods".GetLocalizedString(),
                PageViewModelFactory = async () =>
                    await Task.FromResult(Ioc.Default.GetRequiredService<GameListViewModel>())
            },
            new MainMenuItemViewModel()
            {
                ToolTip = "Search".GetLocalizedString(),
                Icon = MaterialIconKind.Magnify,
                Text = "Search".GetLocalizedString(),
                PageViewModelFactory = async () =>
                    await Task.FromResult(Ioc.Default.GetRequiredService<GameSearchViewModel>())
            },
            new MainMenuItemViewModel()
            {
                ToolTip = "Random".GetLocalizedString(),
                Icon = MaterialIconKind.Gambling,
                Text = "Random game".GetLocalizedString(),
                PageViewModelFactory = async () =>
                    await Task.FromResult(Ioc.Default.GetRequiredService<RandomGameViewModel>())
            },
            new MainMenuItemViewModel()
            {
                ToolTip = "Statistics".GetLocalizedString(),
                Icon = MaterialIconKind.ChartBar,
                Text = "Statistics".GetLocalizedString(),
                PageViewModelFactory = async () => await Task.FromResult(Ioc.Default.GetRequiredService<StatisticsPageViewModel>())
            }
            
        };

        SettingsItem = new MainMenuItemViewModel()
        {
            ToolTip = "Settings".GetLocalizedString(),
            Icon = MaterialIconKind.Settings,
            Text = "Settings".GetLocalizedString(),
            PageViewModelFactory = async () =>
                await Task.FromResult(Ioc.Default.GetRequiredService<SettingsPageViewModel>())
        };

        GitHubLinkItem = new CommandViewModel()
        {
            Tooltip = "Open Tomb Launcher's GitHub page".GetLocalizedString(),
            Icon = MaterialIconKind.Github,
            Text = "GitHub",
            Command = new RelayCommand(OpenGithub)
        };

        _navigationManager.StartNavigation(MenuItems.First().PageViewModelFactory.Invoke().GetAwaiter().GetResult());
        Title = "Tomb Launcher";
    }

    private void OnNavigated()
    {
        OnPropertyChanged(nameof(CurrentPage));
        Dispatcher.UIThread.Invoke(() => ((RelayCommand)GoBackCmd).NotifyCanExecuteChanged());
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
        AppUtils.OpenUrl(gitHubLink);
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
                _navigationManager.StartNavigation(value.PageViewModelFactory.Invoke().GetAwaiter().GetResult());
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
    public ICommand OpenSettingsCmd { get; }

    private async Task OpenSettings()
    {
        await SettingsItem.PageViewModelFactory();
        SelectedMenuItem = SettingsItem;
        IsSettingsOpen = true;
    }

    
}