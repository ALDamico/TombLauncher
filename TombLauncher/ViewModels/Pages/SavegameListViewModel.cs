using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using TombLauncher.Services;

namespace TombLauncher.ViewModels.Pages;

public partial class SavegameListViewModel : PageViewModel
{
    public SavegameListViewModel()
    {
        _savegameService = Ioc.Default.GetRequiredService<SavegameService>();
        Initialize+= OnInitialize;
    }

    private async void OnInitialize()
    {
        SetBusy("Loading savegames");
        await _savegameService.LoadSaveGames(this);
        SetBusy(false);
    }

    [ObservableProperty] private string _gameTitle;
    [ObservableProperty] private int _gameId;
    [ObservableProperty] private ObservableCollection<SavegameViewModel> _savegames = new ObservableCollection<SavegameViewModel>();
    private SavegameService _savegameService;
}