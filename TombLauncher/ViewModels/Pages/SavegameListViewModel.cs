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
using TombLauncher.ViewModels;

namespace TombLauncher.ViewModels.Pages;

public partial class SavegameListViewModel : PageViewModel
{
    public SavegameListViewModel(SavegameQueryService savegameQueryService, SavegameCommandService savegameCommandService)
    {
        _savegameQueryService = savegameQueryService;
        _savegameCommandService = savegameCommandService;
        SavegameFilter = new SaveGameListFilter();

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
    }

    public override async Task OnNavigatedTo(object parameter)
    {
        if (parameter is GameMetadataViewModel gameMetadata)
        {
            GameId = gameMetadata.Id;
            GameTitle = gameMetadata.Title;
            InstallLocation = gameMetadata.InstallDirectory;
            GameEngine = gameMetadata.GameEngine;

            SetBusy("Loading savegames");
            await _savegameQueryService.LoadSaveGames(this);
            await _savegameQueryService.InitSlots(this);
            SetBusy(false);
        }
    }

    [ObservableProperty] private string _gameTitle = string.Empty;
    [ObservableProperty] private int _gameId;
    [ObservableProperty] private GameEngine _gameEngine;
    [ObservableProperty] private string? _installLocation;

    [ObservableProperty]
    private ObservableCollection<SavegameViewModel> _savegames = new ObservableCollection<SavegameViewModel>();

    [ObservableProperty] private ObservableCollection<SavegameViewModel> _filteredSaves = new ObservableCollection<SavegameViewModel>();
    [ObservableProperty] private ObservableCollection<SavegameSlotViewModel> _slots = new ObservableCollection<SavegameSlotViewModel>();
    [ObservableProperty] private SavegameSlotViewModel? _selectedSlot;
    [ObservableProperty] private SaveGameListFilter _savegameFilter;
    private SavegameQueryService _savegameQueryService;
    private SavegameCommandService _savegameCommandService;

    [ObservableProperty] public ICommand _filterCmd;

    private async Task Filter(SaveGameListFilter? slotNumber)
    {
        if (_savegameQueryService == null)
            return;
        if (slotNumber != null)
            await _savegameQueryService.ApplyFilter(this, slotNumber);
    }

    public ICommand UpdateStartOfLevelStateCmd { get; private set; }

    private async Task UpdateStartOfLevelState(SavegameViewModel? targetSaveGame)
    {
        if (targetSaveGame != null)
            await _savegameCommandService.UpdateStartOfLevelState(this, targetSaveGame);
    }

    public IRelayCommand DeleteSaveCmd { get; private set; }

    private async Task DeleteSave(SavegameViewModel? target)
    {
        if (target != null)
            await _savegameCommandService.DeleteSavegame(this, target);
    }

    private bool CanDelete(SavegameViewModel? obj)
    {
        if (obj == null) return false;
        return !obj.IsStartOfLevel;
    }

    public ICommand RestoreSavegameCmd { get; private set; }

    private async Task RestoreSavegame(int savegameId)
    {
        await _savegameCommandService.Restore(this, savegameId, Slots.Max(s => s.SaveSlot).GetValueOrDefault());
    }

    public ICommand DeleteAllCmd { get; private set; }

    private async Task DeleteAll()
    {
        await _savegameCommandService.DeleteAllSavegamesByGameId(this, GameId);
    }

    public ICommand CheckNonBackedUpSavegamesCmd { get; private set; }

    private async Task CheckNonBackedUpSavegames()
    {
        await _savegameQueryService.CheckSavegamesNotBackedUp(this);
    }
}