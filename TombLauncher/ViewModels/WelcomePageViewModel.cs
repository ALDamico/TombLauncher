using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using TombLauncher.Localization;
using TombLauncher.Services;
using TombLauncher.ViewModels.Dialogs;

namespace TombLauncher.ViewModels;

public partial class WelcomePageViewModel : PageViewModel
{
    public WelcomePageViewModel(WelcomePageService welcomePageService)
    {
        _welcomePageService = welcomePageService;
        InitCmd = new RelayCommand(InitializeInner);
    }

    private WelcomePageService _welcomePageService;

    private void InitializeInner()
    {
        _welcomePageService.HandleNotNotifiedCrashes();
    }

    [ObservableProperty] private string _changeLogPath;
}