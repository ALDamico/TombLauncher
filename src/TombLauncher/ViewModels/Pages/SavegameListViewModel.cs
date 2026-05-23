using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using IconPacks.Avalonia.RemixIcon;
using TombLauncher.Contracts.Enums;
using TombLauncher.Localization.Extensions;
using TombLauncher.Services;

namespace TombLauncher.ViewModels.Pages;

public partial class SavegameListViewModel : PageViewModel
{
    public SavegameListViewModel(SavegameQueryService savegameQueryService, SavegameCommandService savegameCommandService)
    {
        _savegameQueryService = savegameQueryService;
        _savegameCommandService = savegameCommandService;
        SavegameFilter = new SaveGameListFilter();

        TopBarCommands.Add(new CommandViewModel()
        {
            Command = DeleteAllCommand,
            Icon = PackIconRemixIconKind.DeleteBinLine,
            Tooltip = "DELETE_ALL".GetLocalizedString()
        });
        TopBarCommands.Add(new CommandViewModel()
        {
            Command = CheckNonBackedUpSavegamesCommand,
            Icon = PackIconRemixIconKind.ImportLine,
            Tooltip = "IMPORT_MISSING_SAVEGAMES".GetLocalizedString()
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
            
            using (BusyScope("LOADING_SAVEGAMES"))
            {
                await _savegameQueryService.LoadSaveGames(this);
                await _savegameQueryService.InitSlots(this);
            }
        }
    }

    [ObservableProperty]
    public partial string GameTitle { get; set; } = string.Empty;
    [ObservableProperty]
    public partial int GameId { get; set; }
    [ObservableProperty]
    public partial GameEngine GameEngine { get; set; }
    [ObservableProperty]
    public partial string? InstallLocation { get; set; }
    [ObservableProperty]
    public partial ObservableCollection<SavegameViewModel> Savegames { get; set; } = [];

    [ObservableProperty]
    public partial ObservableCollection<SavegameViewModel> FilteredSaves { get; set; } = [];

    [ObservableProperty]
    public partial ObservableCollection<SavegameSlotViewModel> Slots { get; set; } = [];

    [ObservableProperty]
    public partial SavegameSlotViewModel? SelectedSlot { get; set; }

    [ObservableProperty]
    public partial SaveGameListFilter SavegameFilter { get; set; }

    private readonly SavegameQueryService _savegameQueryService;
    private readonly SavegameCommandService _savegameCommandService;

    [RelayCommand]
    private async Task Filter(SaveGameListFilter? slotNumber)
    {
        if (slotNumber != null)
            await _savegameQueryService.ApplyFilter(this, slotNumber);
    }

    [RelayCommand]
    private async Task UpdateStartOfLevelState(SavegameViewModel? targetSaveGame)
    {
        if (targetSaveGame != null)
            await _savegameCommandService.UpdateStartOfLevelState(this, targetSaveGame);
    }

    [RelayCommand(AllowConcurrentExecutions = false, CanExecute = nameof(CanDelete))]
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

    [RelayCommand]
    private async Task RestoreSavegame(int savegameId)
    {
        await _savegameCommandService.Restore(this, savegameId, Slots.Max(s => s.SaveSlot).GetValueOrDefault());
    }

    [RelayCommand]
    private async Task DeleteAll()
    {
        await _savegameCommandService.DeleteAllSavegamesByGameId(this, GameId);
    }

    [RelayCommand]
    private async Task CheckNonBackedUpSavegames()
    {
        await _savegameQueryService.CheckSavegamesNotBackedUp(this);
    }
}