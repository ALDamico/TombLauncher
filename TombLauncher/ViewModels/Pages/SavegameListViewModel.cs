using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using Material.Icons;
using TombLauncher.Contracts.Enums;
using TombLauncher.Localization.Extensions;
using TombLauncher.Services;

namespace TombLauncher.ViewModels.Pages;

public partial class SavegameListViewModel : PageViewModel
{
    public SavegameListViewModel()
    {
        SavegameFilter = new SaveGameListFilter();
    }
    
    protected override async Task RaiseInitialize()
    {
        SetBusy("Loading savegames");
        _savegameService = Ioc.Default.GetRequiredService<SavegameService>();
        
        FilterCmd = new AsyncRelayCommand<SaveGameListFilter>(Filter);
        UpdateStartOfLevelStateCmd = new AsyncRelayCommand<SavegameViewModel>(UpdateStartOfLevelState);
        RestoreSavegameCmd = new AsyncRelayCommand<int>(RestoreSavegame);
        DeleteAllCmd = new AsyncRelayCommand(DeleteAll);
        CheckNonBackedUpSavegamesCmd = new AsyncRelayCommand(CheckNonBackedUpSavegames);
        
        DeleteSaveCmd = new AsyncRelayCommand<SavegameViewModel>(DeleteSave, CanDelete);
        TopBarCommands.Add(new CommandViewModel()
        {
            Command = DeleteAllCmd, 
            Icon = MaterialIconKind.Delete, 
            Tooltip = "Delete all".GetLocalizedString()
        });
        TopBarCommands.Add(new CommandViewModel()
        {
            Command = CheckNonBackedUpSavegamesCmd, 
            Icon = MaterialIconKind.Import,
            Tooltip = "Import missing savegames".GetLocalizedString()
        });
        
        await _savegameService.LoadSaveGames(this);
        await _savegameService.InitSlots(this);
        SetBusy(false);
    }

    [ObservableProperty] private string _gameTitle;
    [ObservableProperty] private int _gameId;
    [ObservableProperty] private GameEngine _gameEngine;
    [ObservableProperty] private string _installLocation;

    [ObservableProperty]
    private ObservableCollection<SavegameViewModel> _savegames = new ObservableCollection<SavegameViewModel>();

    [ObservableProperty] private ObservableCollection<SavegameViewModel> _filteredSaves;
    [ObservableProperty] private ObservableCollection<SavegameSlotViewModel> _slots;
    [ObservableProperty] private SavegameSlotViewModel _selectedSlot;
    [ObservableProperty] private SaveGameListFilter _savegameFilter;
    private SavegameService _savegameService;

    [ObservableProperty] public ICommand _filterCmd;

    private async Task Filter(SaveGameListFilter slotNumber)
    {
        if (_savegameService == null)
            return;
        await _savegameService.ApplyFilter(this, slotNumber);
    }
    
    public ICommand UpdateStartOfLevelStateCmd { get; private set; }

    private async Task UpdateStartOfLevelState(SavegameViewModel targetSaveGame)
    {
        await _savegameService.UpdateStartOfLevelState(this, targetSaveGame);
    }
    
    public IRelayCommand DeleteSaveCmd { get; private set; }

    private async Task DeleteSave(SavegameViewModel target)
    {
        await _savegameService.DeleteSavegame(this, target);
    }
    
    private bool CanDelete(SavegameViewModel obj)
    {
        if (obj == null) return false;
        return !obj.IsStartOfLevel;
    }
    
    public ICommand RestoreSavegameCmd { get; private set; }

    private async Task RestoreSavegame(int savegameId)
    {
        await _savegameService.Restore(this, savegameId, Slots.Max(s => s.SaveSlot).GetValueOrDefault());
    }
    
    public ICommand DeleteAllCmd { get; private set; }

    private async Task DeleteAll()
    {
        await _savegameService.DeleteAllSavegamesByGameId(this, GameId);
    }
    
    public ICommand CheckNonBackedUpSavegamesCmd { get; private set; }

    private async Task CheckNonBackedUpSavegames()
    {
        await _savegameService.CheckSavegamesNotBackedUp(this);
    }
}