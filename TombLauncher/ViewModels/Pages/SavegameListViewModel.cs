using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using Material.Icons;
using TombLauncher.Localization.Extensions;
using TombLauncher.Services;

namespace TombLauncher.ViewModels.Pages;

public partial class SavegameListViewModel : PageViewModel
{
    public SavegameListViewModel()
    {
        _savegameService = Ioc.Default.GetRequiredService<SavegameService>();
        SavegameFilter = new SaveGameListFilter();
        FilterCmd = new AsyncRelayCommand<SaveGameListFilter>(Filter);
        UpdateStartOfLevelStateCmd = new AsyncRelayCommand<SavegameViewModel>(UpdateStartOfLevelState);
        DeleteSaveCmd = new AsyncRelayCommand<SavegameViewModel>(DeleteSave, CanDelete);
        RestoreSavegameCmd = new AsyncRelayCommand<int>(RestoreSavegame);
        //Initialize += OnInitialize;
        _savegameService.NavigationManager.OnNavigated += OnInitialize;
        DeleteAllCmd = new AsyncRelayCommand(DeleteAll);
        TopBarCommands.Add(new CommandViewModel(){Command = DeleteAllCmd, Icon = MaterialIconKind.Delete, Tooltip = "Delete all".GetLocalizedString()});
    }
    
    private async void OnInitialize()
    {
        SetBusy("Loading savegames");
        if (!_savegamesChecked)
            await _savegameService.CheckSavegamesNotBackedUp(this);
        _savegamesChecked = true;
        await _savegameService.LoadSaveGames(this);
        await _savegameService.InitSlots(this);
        SetBusy(false);
    }

    [ObservableProperty] private string _gameTitle;
    [ObservableProperty] private int _gameId;
    [ObservableProperty] private string _installLocation;
    private bool _savegamesChecked;

    [ObservableProperty]
    private ObservableCollection<SavegameViewModel> _savegames = new ObservableCollection<SavegameViewModel>();

    [ObservableProperty] private ObservableCollection<SavegameViewModel> _filteredSaves;
    [ObservableProperty] private ObservableCollection<SavegameSlotViewModel> _slots;
    [ObservableProperty] private SavegameSlotViewModel _selectedSlot;
    [ObservableProperty] private SaveGameListFilter _savegameFilter;
    private SavegameService _savegameService;

    public ICommand FilterCmd { get; }

    private async Task Filter(SaveGameListFilter slotNumber)
    {
        await _savegameService.ApplyFilter(this, slotNumber);
    }
    
    public ICommand UpdateStartOfLevelStateCmd { get; }

    private async Task UpdateStartOfLevelState(SavegameViewModel targetSaveGame)
    {
        await _savegameService.UpdateStartOfLevelState(this, targetSaveGame);
    }
    
    public ICommand DeleteSaveCmd { get; set; }

    private async Task DeleteSave(SavegameViewModel target)
    {
        await _savegameService.DeleteSavegame(this, target);
    }
    
    private bool CanDelete(SavegameViewModel obj)
    {
        if (obj == null) return false;
        return !obj.IsStartOfLevel;
    }
    
    public ICommand RestoreSavegameCmd { get; }

    private async Task RestoreSavegame(int savegameId)
    {
        await _savegameService.Restore(savegameId, Slots.Max(s => s.SaveSlot).GetValueOrDefault());
    }
    
    public ICommand DeleteAllCmd { get; }

    private async Task DeleteAll()
    {
        await _savegameService.DeleteAllSavegamesByGameId(GameId);
    }
}