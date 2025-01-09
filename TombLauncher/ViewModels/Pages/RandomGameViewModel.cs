using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using TombLauncher.Localization.Extensions;
using TombLauncher.Services;

namespace TombLauncher.ViewModels.Pages;

public partial class RandomGameViewModel : PageViewModel
{
    public RandomGameViewModel()
    {
        _settingsService = Ioc.Default.GetRequiredService<SettingsService>();
        _randomGameService = Ioc.Default.GetRequiredService<RandomGameService>();
        PickRandomGameCmd = new AsyncRelayCommand(PickRandomGame);
        Initialize += OnInitialize;
    }

    private readonly RandomGameService _randomGameService;
    private readonly SettingsService _settingsService;
    [ObservableProperty] private bool _attemptsExpired;
    [ObservableProperty] private int _maxRetries;

    private async void OnInitialize()
    {
        await PickRandomGame();
    }
    
    public ICommand PickRandomGameCmd { get; }

    private async Task PickRandomGame()
    {
        AttemptsExpired = false;
        IsBusy = true;
        BusyMessage = "Picking a random game for you...".GetLocalizedString();
        MaxRetries = _settingsService.GetRandomGameMaxRerolls();
        await _randomGameService.PickRandomGame(this);
    }
}