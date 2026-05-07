using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using JamSoft.AvaloniaUI.Dialogs.MsgBox;
using TombLauncher.Contracts.Enums;
using TombLauncher.Core.Dtos;
using TombLauncher.Core.Utils;
using TombLauncher.Core.Extensions;
using AvaloniaEdit.Utils;
using TombLauncher.Data.Database.Repositories;
using TombLauncher.Extensions;
using TombLauncher.Localization.Extensions;
using TombLauncher.ViewModels;
using TombLauncher.ViewModels.Pages;

namespace TombLauncher.Services;

public class SavegameQueryService
{
    private readonly ISavegameRepository _savegameRepository;
    private readonly ISavegameHeaderProvider _headerProvider;
    private readonly IPopupService _popupService;
    private readonly int? _numberOfVersionsToKeep;

    public SavegameQueryService(
        ISavegameRepository savegameRepository,
        ISavegameHeaderProvider headerProvider,
        IPopupService popupService,
        ISettingsProvider settingsProvider)
    {
        _savegameRepository = savegameRepository;
        _headerProvider = headerProvider;
        _popupService = popupService;
        _numberOfVersionsToKeep = settingsProvider.GetSavegameSettings().NumberOfVersionsToKeep;
    }

    public async Task LoadSaveGames(SavegameListViewModel targetViewModel)
    {
        using (targetViewModel.BusyScope("FETCHING_SAVEGAMES_FOR_GAMETITLE".GetLocalizedString(targetViewModel.GameTitle)))
        {
            var observableCollection = new ObservableCollection<SavegameViewModel>();
            var knownSavegames = await _savegameRepository.GetSavegamesByGameId(targetViewModel.GameId);
            var headerParser = _headerProvider.GetHeaderReader(targetViewModel.GameEngine);
            foreach (var savegame in knownSavegames)
            {
                var savegameHeader = headerParser.ReadHeader(savegame.FileName, savegame.Data!);
                var viewModel = new SavegameViewModel()
                {
                    UpdateStartOfLevelStateCommand = targetViewModel.UpdateStartOfLevelStateCommand,
                    DeleteSavegameCommand = targetViewModel.DeleteSaveCommand,
                    RestoreSavegameCommand = targetViewModel.RestoreSavegameCommand,
                    Id = savegame.Id,
                    Filename = savegame.FileName,
                    LevelName = savegameHeader.LevelName,
                    SaveNumber = savegameHeader.SaveNumber,
                    IsStartOfLevel = savegame.FileType == FileType.SavegameStartOfLevel,
                    SlotNumber = savegameHeader.SlotNumber,
                    Length = savegame.Data!.LongLength,
                    BackedUpOn = savegame.BackedUpOn
                };
                observableCollection.Add(viewModel);
            }

            observableCollection = observableCollection.OrderBy(f => f.SlotNumber)
                .ThenBy(f => f.BackedUpOn)
                .ToObservableCollection();

            targetViewModel.Savegames = observableCollection;
            targetViewModel.FilteredSaves = observableCollection;
        }
    }

    public Task InitSlots(SavegameListViewModel savegameListViewModel)
    {
        var usedSlots = savegameListViewModel.Savegames.Select(sg => sg.SlotNumber)
            .Distinct()
            .OrderBy(s => s)
            .ToList();
        savegameListViewModel.Slots = new ObservableCollection<SavegameSlotViewModel>();
        var allSlotsItem = new SavegameSlotViewModel()
        {
            Header = "ALL_SLOTS".GetLocalizedString(),
            IsEnabled = true,
            SaveSlot = null,
            FilterCommand = savegameListViewModel.FilterCommand
        };
        savegameListViewModel.Slots.Add(allSlotsItem);

        savegameListViewModel.Slots.Add(new SavegameSlotViewModel()
        {
            Header = "-----",
            IsEnabled = false,
            SaveSlot = null,
            FilterCommand = null
        });

        savegameListViewModel.Slots.AddRange(usedSlots.Select(s => new SavegameSlotViewModel()
        {
            Header = "SLOT_NUMBER_FORMATTABLE".GetLocalizedString(s),
            SaveSlot = s,
            IsEnabled = true,
            FilterCommand = savegameListViewModel.FilterCommand
        }));
        savegameListViewModel.SelectedSlot = allSlotsItem;
        return Task.CompletedTask;
    }

    public Task ApplyFilter(SavegameListViewModel savegameListViewModel, SaveGameListFilter filter)
    {
        using (savegameListViewModel.BusyScope("FILTERING_SAVEGAMES".GetLocalizedString()))
        {
            var slotNumber = filter.SlotNumber;
            var startOfLevelOnly = filter.StartOfLevelOnly;
            var savegameEnumerable = savegameListViewModel.Savegames.AsEnumerable();
            if (startOfLevelOnly)
            {
                savegameEnumerable = savegameEnumerable.Where(sg => sg.IsStartOfLevel);
            }

            if (slotNumber == null)
            {
                savegameListViewModel.FilteredSaves = savegameEnumerable.ToObservableCollection();
                return Task.CompletedTask;
            }

            var savegamesBySlot = savegameEnumerable.Where(sg => sg.SlotNumber == slotNumber)
                .OrderBy(sg => sg.BackedUpOn)
                .ToObservableCollection();
            savegameListViewModel.FilteredSaves = savegamesBySlot;
            return Task.CompletedTask;
        }
    }

    public async Task CheckSavegamesNotBackedUp(SavegameListViewModel savegameListView)
    {
        var installDir = savegameListView.InstallLocation;
        if (installDir == null) return;

        var savegames = Directory.GetFiles(installDir, "save*.*", SearchOption.AllDirectories)
            .Where(f => Path.GetExtension(f).TrimStart('.').All(char.IsDigit) || Path.GetExtension(f).TrimStart('.') == "dat")
            .ToList();
        var existingGamesDict = new Dictionary<string, string>();
        foreach (var savegameFile in savegames)
        {
            var fileContent = await File.ReadAllBytesAsync(savegameFile);
            var md5 = CryptoUtils.ComputeMd5Hash(fileContent);
            existingGamesDict[md5] = savegameFile;
        }

        var backedUpSaves = await _savegameRepository.GetSavegameMd5HashesByGameId(savegameListView.GameId);
        var missingSaveGames = existingGamesDict.Keys.Except(backedUpSaves).Intersect(existingGamesDict.Keys).ToList();
        if (missingSaveGames.Count == 0)
        {
            await _popupService.ShowLocalized("Scan complete",
                "There were no savegames to import.", MsgBoxButton.Ok, MsgBoxImage.Information);
            return;
        }

        var userResponse = await _popupService.Show("NO_SAVEGAME_BACKUPS_FOUND".GetLocalizedString(),
            "SAVEGAME_BACKUPS_FOUND_IMPORT".GetLocalizedString(missingSaveGames.Count),
            MsgBoxButton.YesNo, MsgBoxImage.Folder, "NO".GetLocalizedString(), "YES".GetLocalizedString());
        if (userResponse.ButtonResult == MsgBoxButtonResult.Yes)
        {
            using (savegameListView.BusyScope("IMPORTING_SAVEGAMES".GetLocalizedString()))
            {
                var headerReader = _headerProvider.GetHeaderReader(savegameListView.GameEngine);
                var dataToBackup = new List<SavegameBackupDto>();
                foreach (var file in savegames)
                {
                    var data = headerReader.ReadHeader(file);
                    var savegameBytes = await File.ReadAllBytesAsync(file);
                    var dto = new SavegameBackupDto()
                    {
                        Data = savegameBytes,
                        FileName = file,
                        FileType = FileType.Savegame,
                        BackedUpOn = DateTime.Now,
                        Md5 = CryptoUtils.ComputeMd5Hash(savegameBytes),
                        LevelName = data.LevelName,
                        SaveNumber = data.SaveNumber,
                        SlotNumber = data.SlotNumber
                    };
                    dataToBackup.Add(dto);
                }

                _savegameRepository.BackupSavegames(savegameListView.GameId, savegameListView.GameEngine, dataToBackup, _numberOfVersionsToKeep);
                await _savegameRepository.Save();
            }
        }
    }
}
