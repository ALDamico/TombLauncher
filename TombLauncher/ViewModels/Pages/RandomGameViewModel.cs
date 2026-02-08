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
    public RandomGameViewModel(RandomGameService randomGameService)
    {
        _randomGameService = randomGameService;
        PickRandomGameCmd = new AsyncRelayCommand(PickRandomGame);
    }

    private readonly RandomGameService _randomGameService;
    [ObservableProperty] private bool _attemptsExpired;
    [ObservableProperty] private int _maxRetries;

    public override async Task OnNavigatedTo(object parameter) => await PickRandomGame();

    public ICommand PickRandomGameCmd { get; }

    private async Task PickRandomGame() => await _randomGameService.PickRandomGame(this);
}