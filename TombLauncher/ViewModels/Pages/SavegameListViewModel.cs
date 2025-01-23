using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using TombLauncher.Services;

namespace TombLauncher.ViewModels.Pages;

public partial class SavegameListViewModel : PageViewModel
{
    public SavegameListViewModel()
    {
        _savegameService = Ioc.Default.GetRequiredService<SavegameService>();
        FilterCmd = new AsyncRelayCommand<int?>(Filter);
        Initialize += OnInitialize;
    }

    private async void OnInitialize()
    {
        SetBusy("Loading savegames");
        await _savegameService.CheckSavegamesNotBackedUp(this);
        await _savegameService.LoadSaveGames(this);
        await _savegameService.InitSlots(this);
        SetBusy(false);
    }

    [ObservableProperty] private string _gameTitle;
    [ObservableProperty] private int _gameId;
    [ObservableProperty] private string _installLocation;

    [ObservableProperty]
    private ObservableCollection<SavegameViewModel> _savegames = new ObservableCollection<SavegameViewModel>();

    [ObservableProperty] private ObservableCollection<SavegameViewModel> _filteredSaves;
    [ObservableProperty] private ObservableCollection<SavegameSlotViewModel> _slots;
    [ObservableProperty] private SavegameSlotViewModel _selectedSlot;
    private SavegameService _savegameService;

    public ICommand FilterCmd { get; }

    private async Task Filter(int? slotNumber)
    {
        await _savegameService.ApplyFilter(this, slotNumber);
    }
}