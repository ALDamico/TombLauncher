using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using IconPacks.Avalonia.RemixIcon;
using TombLauncher.Core.Navigation;
using TombLauncher.Core.PlatformSpecific;
using TombLauncher.Localization.Extensions;
using TombLauncher.Services;
using TombLauncher.ViewModels.Pages;

namespace TombLauncher.ViewModels;

public partial class MainWindowViewModel : WindowViewModelBase
{
    public MainWindowViewModel(NavigationManager navigationManager, NotificationListViewModel notificationListViewModel, ISettingsProvider settingsProvider, IPlatformSpecificFeatures platformSpecificFeatures)
    {
        _navigationManager = navigationManager;
        _navigationManager.PropertyChanged += NavigationManagerOnPropertyChanged;
        NotificationListViewModel = notificationListViewModel;
        _settingsProvider = settingsProvider;
        _platformSpecificFeatures = platformSpecificFeatures;
        TogglePaneCmd = new RelayCommand(TogglePane);
        GoBackCmd = new RelayCommand(GoBack, CanGoBack);
        OpenSettingsCmd = new AsyncRelayCommand(OpenSettings);
        MenuItems = new ObservableCollection<MainMenuItemViewModel>()
        {
            new MainMenuItemViewModel()
            {
                ToolTip = "WELCOME".GetLocalizedString(),
                Icon = PackIconRemixIconKind.HomeLine,
                Text = "WELCOME".GetLocalizedString(),
                ViewModelType = typeof(WelcomePageViewModel)
            },
            new MainMenuItemViewModel()
            {
                ToolTip = "MY_MODS".GetLocalizedString(),
                Icon = PackIconRemixIconKind.GamepadLine,
                Text = "MY_MODS".GetLocalizedString(),
                ViewModelType = typeof(GameListViewModel)
            },
            new MainMenuItemViewModel()
            {
                ToolTip = "SEARCH".GetLocalizedString(),
                Icon = PackIconRemixIconKind.Search2Line,
                Text = "SEARCH".GetLocalizedString(),
                ViewModelType = typeof(GameSearchViewModel)
            },
            new MainMenuItemViewModel()
            {
                ToolTip = "STATISTICS".GetLocalizedString(),
                Icon = PackIconRemixIconKind.BarChart2Line,
                Text = "STATISTICS".GetLocalizedString(),
                ViewModelType = typeof(StatisticsPageViewModel)
            },
            new MainMenuItemViewModel()
            {
                ToolTip = "TALK_TO_LAURA".GetLocalizedString(),
                Icon = PackIconRemixIconKind.ChatAiLine,
                Text = "TALK_TO_LAURA".GetLocalizedString(),
                ViewModelType = typeof(AiChatViewModel)
            }
        };

        SettingsItem = new MainMenuItemViewModel()
        {
            ToolTip = "SETTINGS".GetLocalizedString(),
            Icon = PackIconRemixIconKind.SettingsLine,
            Text = "SETTINGS".GetLocalizedString(),
            ViewModelType = typeof(SettingsPageViewModel)
        };

        GitHubLinkItem = new CommandViewModel()
        {
            Tooltip = "OPEN_TOMB_LAUNCHER_S_GITHUB_PAGE".GetLocalizedString(),
            Icon = PackIconRemixIconKind.GithubLine,
            Text = "GitHub",
            Command = new RelayCommand(OpenGithub)
        };

        Title = "Tomb Launcher";
        // Initialize default view
        SelectedMenuItem = MenuItems.First();
    }

    private bool _isSyncingSelection;
    private void NavigationManagerOnPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(NavigationManager.CurrentPage))
        {
            OnPropertyChanged(nameof(CurrentPage));
            // Sync sidebar selection when navigation happens programmatically
            if (!_isSyncingSelection)
            {
                var currentPageType = _navigationManager.CurrentPage?.GetType();
                var matchingItem = MenuItems.FirstOrDefault(m => m.ViewModelType == currentPageType);
                if (matchingItem != null && matchingItem != SelectedMenuItem)
                {
                    _isSyncingSelection = true;
                    SelectedMenuItem = matchingItem;
                    _isSyncingSelection = false;
                }
            }
        }

        if (e.PropertyName == nameof(NavigationManager.CanGoBack))
        {
            Dispatcher.UIThread.Invoke(() => ((RelayCommand)GoBackCmd).NotifyCanExecuteChanged());
        }
    }

    private readonly NavigationManager _navigationManager;
    private readonly ISettingsProvider _settingsProvider;
    private readonly IPlatformSpecificFeatures _platformSpecificFeatures;
    [ObservableProperty] private NotificationListViewModel _notificationListViewModel;
    [ObservableProperty] private MainMenuItemViewModel _settingsItem;
    [ObservableProperty] private CommandViewModel _gitHubLinkItem;
    [ObservableProperty] private bool _isSettingsOpen;

    private void OpenGithub()
    {
        var gitHubLink = _settingsProvider.GetApplicationSettings().GitHubLink;
        _platformSpecificFeatures.OpenUrl(gitHubLink);
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

    public MainMenuItemViewModel? SelectedMenuItem
    {
        get;
        set
        {
            if (value != SettingsItem)
                IsSettingsOpen = false;
            SetProperty(ref field, value);
            if (value != null && !_isSyncingSelection)
            {
                // Using NavigateToRoot for main menu items to clear history stack
                _ = _navigationManager.NavigateToRoot(value.ViewModelType!);
            }
        }
    } = null!;


    public INavigationTarget? CurrentPage => _navigationManager.CurrentPage as INavigationTarget;
    public ICommand GoBackCmd { get; }

    private void GoBack()
    {
        _ = _navigationManager.GoBack();
    }

    private bool CanGoBack() => _navigationManager.CanGoBack;
    public ICommand OpenSettingsCmd { get; }

    private async Task OpenSettings()
    {
        await _navigationManager.NavigateTo(SettingsItem.ViewModelType!);
        SelectedMenuItem = SettingsItem;
        IsSettingsOpen = true;
    }
}