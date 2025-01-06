using System;
using System.Threading;
using CommunityToolkit.Mvvm.DependencyInjection;
using TombLauncher.Contracts.Downloaders;
using TombLauncher.Core.Extensions;
using TombLauncher.Installers.Downloaders;
using TombLauncher.Services;

namespace TombLauncher.ViewModels.Pages;

public class RandomGameViewModel : PageViewModel
{
    public RandomGameViewModel()
    {
        _settingsService = Ioc.Default.GetRequiredService<SettingsService>();
        _randomGameService = Ioc.Default.GetRequiredService<RandomGameService>();
        Initialize += OnInitialize;
    }

    private RandomGameService _randomGameService;

    private SettingsService _settingsService;

    private async void OnInitialize()
    {
        IsBusy = true;
        BusyMessage = "Picking a random game for you";
        await _randomGameService.PickRandomGame();
    }
}