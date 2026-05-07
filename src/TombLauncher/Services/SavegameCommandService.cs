using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using JamSoft.AvaloniaUI.Dialogs.MsgBox;
using Microsoft.Extensions.Logging;
using TombLauncher.Contracts.Enums;
using TombLauncher.Core.Utils;
using TombLauncher.Data.Database.Repositories;
using TombLauncher.Extensions;
using TombLauncher.Localization.Extensions;
using TombLauncher.Mappers;
using TombLauncher.ViewModels;
using TombLauncher.ViewModels.Dialogs;
using TombLauncher.ViewModels.Pages;

namespace TombLauncher.Services;

public class SavegameCommandService
{
    private readonly ISavegameRepository _savegameRepository;
    private readonly ISavegameHeaderProvider _headerProvider;
    private readonly IPopupService _popupService;
    private readonly ILogger<SavegameCommandService> _logger;
    private readonly SavegameMapper _savegameMapper;

    public SavegameCommandService(
        ISavegameRepository savegameRepository,
        ISavegameHeaderProvider headerProvider,
        IPopupService popupService,
        ILogger<SavegameCommandService> logger,
        SavegameMapper savegameMapper)
    {
        _savegameRepository = savegameRepository;
        _headerProvider = headerProvider;
        _popupService = popupService;
        _logger = logger;
        _savegameMapper = savegameMapper;
    }

    public async Task UpdateStartOfLevelState(SavegameListViewModel savegameListViewModel,
        SavegameViewModel targetSaveGame)
    {
        _logger.LogInformation("Setting savegame number {Savegame} as start of level...", targetSaveGame.SaveNumber);
        using (savegameListViewModel.BusyScope("UPDATE_IN_PROGRESS".GetLocalizedString()))
        {
            var dto = _savegameMapper.ToDto(targetSaveGame);
            await _savegameRepository.UpdateSavegameStartOfLevel(dto);
        }
    }

    public async Task DeleteSavegame(SavegameListViewModel savegameListViewModel, SavegameViewModel savegameViewModel)
    {
        var userIsSure = await _popupService.Show("CONFIRM_DELETE".GetLocalizedString(),
            "ARE_YOU_SURE_YOU_WANT_TO_DELETE_THIS_SAVEGAME_THIS".GetLocalizedString(),
            MsgBoxButton.YesNo,
            MsgBoxImage.Question);
        if (userIsSure.ButtonResult == MsgBoxButtonResult.No)
        {
            return;
        }

        using (savegameListViewModel.BusyScope("DELETING_SAVEGAME".GetLocalizedString()))
        {
            await _savegameRepository.DeleteFileBackupById(savegameViewModel.Id);
            savegameListViewModel.FilteredSaves.Remove(savegameViewModel);
            savegameListViewModel.Savegames.Remove(savegameViewModel);
        }
    }

    public async Task Restore(SavegameListViewModel savegameListViewModel, int savegameId, int max)
    {
        using (savegameListViewModel.BusyScope("RESTORING_SAVEGAME".GetLocalizedString()))
        {
            var savegame = _savegameRepository.GetSavegameById(savegameId);
            var availableSlots = new ObservableCollection<SavegameSlotViewModel>();
            for (var i = 0; i < max; i++)
            {
                var slotViewModel = new SavegameSlotViewModel()
                {
                    Header = "Slot #" + (i + 1),
                    SaveSlot = i
                };
                availableSlots.Add(slotViewModel);
            }

            var dialogViewModel = new RestoreSavegameDialogViewModel()
            {
                Slots = availableSlots,
                SelectedSlot = availableSlots.FirstOrDefault(s => s.SaveSlot == savegame?.SlotNumber) ?? availableSlots.First(),
                Data = savegame?.Data ?? [],
                TargetDirectory = Path.GetDirectoryName(savegame?.FileName) ?? string.Empty,
                BaseFileName = Path.GetFileNameWithoutExtension(savegame?.FileName) ?? string.Empty
            };
            _popupService.ShowDialog(dialogViewModel, ExecuteRestore);
        }

        await Task.CompletedTask;
    }

    private void ExecuteRestore(RestoreSavegameDialogViewModel vm)
    {
        var selectedSlot = vm.SelectedSlot.SaveSlot;
        _logger.LogInformation("Restoring slot {SelectedSlot}...", selectedSlot);
        var fileName = Path.Combine(vm.TargetDirectory, string.Join(".", vm.BaseFileName, selectedSlot));
        File.WriteAllBytes(fileName, vm.Data);
        _logger.LogInformation("Restores {SelectedSlot}", selectedSlot);
    }

    public async Task DeleteAllSavegamesByGameId(SavegameListViewModel savegameListViewModel, int gameId)
    {
        var result = await _popupService.Show("DELETE_ALL_SAVEGAMES".GetLocalizedString(),
            "ARE_YOU_SURE_YOU_WANT_TO_DELETE_ALL_SAVEGAMES_THIS".GetLocalizedString(),
            MsgBoxButton.OkCancel, MsgBoxImage.Warning,
            checkBoxText: "DELETE_SAVEGAMES_MARKED_AS_START_OF_LEVEL".GetLocalizedString());

        if (result.ButtonResult == MsgBoxButtonResult.Ok)
        {
            using (savegameListViewModel.BusyScope("DELETING_SAVEGAMES".GetLocalizedString()))
            {
                var deleteStartOfLevel = result.CheckBoxResult;
                var targetTypes = new List<FileType>() { FileType.Savegame };
                if (deleteStartOfLevel)
                {
                    targetTypes.Add(FileType.SavegameStartOfLevel);
                }

                _savegameRepository.DeleteFileBackupsByGameId(gameId, targetTypes);
            }
        }
    }

    public async Task SyncSavegames(PageViewModel page)
    {
        using (page.BusyScope("SYNCING_SAVEGAMES".GetLocalizedString()))
        {
            var allGamesWithSaves = await _savegameRepository.GetSavegameBackups();

            foreach (var savegame in allGamesWithSaves)
            {
                var headerReader = _headerProvider.GetHeaderReader(savegame.GameEngine);
                var headerData = headerReader.ReadHeader(savegame.FileName, savegame.Data);
                savegame.LevelName = headerData.LevelName;
                savegame.SlotNumber = headerData.SlotNumber;
                savegame.SaveNumber = headerData.SaveNumber;
                var md5 = CryptoUtils.ComputeMd5Hash(savegame.Data);
                if (savegame.Md5 != md5)
                {
                    savegame.Md5 = md5;
                }
            }

            await _savegameRepository.SyncSavegameMetadata(allGamesWithSaves);
            await _popupService.ShowLocalized("Sync completed", "Synchronization completed successfully!", MsgBoxButton.Ok,
                MsgBoxImage.Success);
        }
    }
}
