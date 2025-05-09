﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using AvaloniaEdit.Utils;
using CommunityToolkit.Mvvm.DependencyInjection;
using JamSoft.AvaloniaUI.Dialogs;
using JamSoft.AvaloniaUI.Dialogs.MsgBox;
using Microsoft.Extensions.Logging;
using TombLauncher.Contracts.Enums;
using TombLauncher.Core.Dtos;
using TombLauncher.Core.Extensions;
using TombLauncher.Core.Savegames.HeaderReaders;
using TombLauncher.Core.Utils;
using TombLauncher.Data.Database.UnitOfWork;
using TombLauncher.Localization.Extensions;
using TombLauncher.ViewModels;
using TombLauncher.ViewModels.Dialogs;
using TombLauncher.ViewModels.Pages;
using Path = System.IO.Path;
using TombLauncher.Extensions;

namespace TombLauncher.Services;

public class SavegameService
{
    public SavegameService()
    {
        _gamesUnitOfWork = Ioc.Default.GetRequiredService<GamesUnitOfWork>();
        _messageBoxService = Ioc.Default.GetRequiredService<IMessageBoxService>();
        _mapper = Ioc.Default.GetRequiredService<MapperConfiguration>().CreateMapper();
        var settingsService = Ioc.Default.GetRequiredService<SettingsService>();
        _numberOfVersionsToKeep = settingsService.GetSavegameSettings(null).NumberOfVersionsToKeep;
        _dialogService = Ioc.Default.GetRequiredService<IDialogService>();
        _logger = Ioc.Default.GetRequiredService<ILogger<SavegameService>>();
        InitHeaderReaderMap();
    }

    private readonly GamesUnitOfWork _gamesUnitOfWork;
    private readonly IMessageBoxService _messageBoxService;
    private readonly IMapper _mapper;
    private int? _numberOfVersionsToKeep;
    private IDialogService _dialogService;
    private Dictionary<GameEngine, ISavegameHeaderReader> _headerReaderMap;
    private ILogger<SavegameService> _logger;

    private void InitHeaderReaderMap()
    {
        var unsupportedHeaderReader = new SavegameUnsupportedHeaderReader();
        var classicGamesHeaderReader = new SavegameHeaderReader();
        var tr1xHeaderReader = new Tr1xSavegameHeaderReader();
        _headerReaderMap = new Dictionary<GameEngine, ISavegameHeaderReader>()
        {
            { GameEngine.Unknown, unsupportedHeaderReader },
            { GameEngine.Ten, unsupportedHeaderReader },
            { GameEngine.Tr1x, tr1xHeaderReader },
            { GameEngine.Tr2x, unsupportedHeaderReader },
            { GameEngine.Tomb2Main, classicGamesHeaderReader },
            { GameEngine.TombAti, classicGamesHeaderReader },
            { GameEngine.TombRaider1, classicGamesHeaderReader },
            { GameEngine.TombRaider2, classicGamesHeaderReader },
            { GameEngine.TombRaider3, classicGamesHeaderReader },
            { GameEngine.TombRaider4, classicGamesHeaderReader },
            { GameEngine.TombRaider5, classicGamesHeaderReader },
            { GameEngine.Tomb3CommunityEdition, classicGamesHeaderReader },
            { GameEngine.TombRaider1Dos, classicGamesHeaderReader }
        };
    }

    public async Task LoadSaveGames(SavegameListViewModel targetViewModel)
    {
        targetViewModel.SetBusy("Fetching savegames for GAMETITLE".GetLocalizedString(targetViewModel.GameTitle));
        var observableCollection = new ObservableCollection<SavegameViewModel>();
        var knownSavegames = await _gamesUnitOfWork.GetSavegamesByGameId(targetViewModel.GameId);
        var headerParser = _headerReaderMap[targetViewModel.GameEngine];
        foreach (var savegame in knownSavegames)
        {
            var savegameHeader = headerParser.ReadHeader(savegame.FileName, savegame.Data);
            var viewModel = new SavegameViewModel()
            {
                UpdateStartOfLevelStateCmd = targetViewModel.UpdateStartOfLevelStateCmd,
                DeleteSavegameCmd = targetViewModel.DeleteSaveCmd,
                RestoreSavegameCmd = targetViewModel.RestoreSavegameCmd,
                Id = savegame.Id,
                Filename = savegame.FileName,
                LevelName = savegameHeader.LevelName,
                SaveNumber = savegameHeader.SaveNumber,
                IsStartOfLevel = savegame.FileType == FileType.SavegameStartOfLevel,
                SlotNumber = savegameHeader.SlotNumber,
                Length = savegame.Data.LongLength,
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

    public Task InitSlots(SavegameListViewModel savegameListViewModel)
    {
        var usedSlots = savegameListViewModel.Savegames.Select(sg => sg.SlotNumber)
            .Distinct()
            .OrderBy(s => s)
            .ToList();
        savegameListViewModel.Slots = new ObservableCollection<SavegameSlotViewModel>();
        var allSlotsItem = new SavegameSlotViewModel()
        {
            Header = "All slots".GetLocalizedString(),
            IsEnabled = true,
            SaveSlot = null,
            FilterCmd = savegameListViewModel.FilterCmd
        };
        savegameListViewModel.Slots.Add(allSlotsItem);

        savegameListViewModel.Slots.Add(new SavegameSlotViewModel()
        {
            Header = "-----",
            IsEnabled = false,
            SaveSlot = null,
            FilterCmd = null
        });

        savegameListViewModel.Slots.AddRange(usedSlots.Select(s => new SavegameSlotViewModel()
        {
            Header = "Slot NUMBER".GetLocalizedString(s),
            SaveSlot = s,
            IsEnabled = true,
            FilterCmd = savegameListViewModel.FilterCmd
        }));
        savegameListViewModel.SelectedSlot = allSlotsItem;
        return Task.CompletedTask;
    }

    public Task ApplyFilter(SavegameListViewModel savegameListViewModel, SaveGameListFilter filter)
    {
        savegameListViewModel.SetBusy("Filtering savegames...".GetLocalizedString());
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
            savegameListViewModel.SetBusy(false);
            return Task.CompletedTask;
        }

        var savegamesBySlot = savegameEnumerable.Where(sg => sg.SlotNumber == slotNumber)
            .OrderBy(sg => sg.BackedUpOn)
            .ToObservableCollection();
        savegameListViewModel.FilteredSaves = savegamesBySlot;
        savegameListViewModel.SetBusy(false);
        return Task.CompletedTask;
    }

    public async Task CheckSavegamesNotBackedUp(SavegameListViewModel savegameListView)
    {
        var installDir = savegameListView.InstallLocation;

        var savegames = Directory.GetFiles(installDir, "save*.*", SearchOption.AllDirectories)
            .Where(f => Path.GetExtension(f).TrimStart('.').All(char.IsDigit) || Path.GetExtension(f).TrimStart('.') == "dat")
            .ToList();
        var existingGamesDict = new Dictionary<string, string>();
        foreach (var savegameFile in savegames)
        {
            var fileContent = await File.ReadAllBytesAsync(savegameFile);
            var md5 = await Md5Utils.ComputeMd5Hash(fileContent);
            existingGamesDict[md5] = savegameFile;
        }

        var backedUpSaves = await _gamesUnitOfWork.GetSavegameMd5sByGameId(savegameListView.GameId);
        var missingSaveGames = existingGamesDict.Keys.Except(backedUpSaves).Intersect(existingGamesDict.Keys).ToList();
        if (missingSaveGames.Count == 0)
        {
            await _messageBoxService.ShowLocalized("Scan complete",
                "There were no savegames to import.", MsgBoxButton.Ok, MsgBoxImage.Information);
            return;
        }

        var userResponse = await _messageBoxService.Show("No savegame backups found",
            $"There were no savegame backups in Tomb Launcher's database, but we found {missingSaveGames.Count()} savegame files. Would you like to import them?",
            MsgBoxButton.YesNo, MsgBoxImage.Folder, "No".GetLocalizedString(), "Yes".GetLocalizedString());
        if (userResponse.ButtonResult == MsgBoxButtonResult.Yes)
        {
            savegameListView.SetBusy("Importing your saved games...");
            var headerReader = _headerReaderMap[savegameListView.GameEngine];
            var dataToBackup = new List<SavegameBackupDto>();
            foreach (var file in savegames)
            {
                var data = headerReader.ReadHeader(file);
                if (data != null)
                {
                    var savegameBytes = await File.ReadAllBytesAsync(file);
                    var dto = new SavegameBackupDto()
                    {
                        Data = savegameBytes,
                        FileName = file,
                        FileType = FileType.Savegame,
                        BackedUpOn = DateTime.Now,
                        Md5 = await Md5Utils.ComputeMd5Hash(savegameBytes),
                        LevelName = data.LevelName,
                        SaveNumber = data.SaveNumber,
                        SlotNumber = data.SlotNumber
                    };
                    dataToBackup.Add(dto);
                }
            }

            _gamesUnitOfWork.BackupSavegames(savegameListView.GameId, savegameListView.GameEngine, dataToBackup, _numberOfVersionsToKeep);
            await _gamesUnitOfWork.Save();
            savegameListView.SetBusy(false);
        }
        else
        {
            savegameListView.SetBusy(false);
        }
    }

    public async Task UpdateStartOfLevelState(SavegameListViewModel savegameListViewModel,
        SavegameViewModel targetSaveGame)
    {
        _logger.LogInformation("Setting savegame number {Savegame} as start of level...", targetSaveGame.SaveNumber);
        savegameListViewModel.SetBusy("Update in progress...");
        var dto = _mapper.Map<FileBackupDto>(targetSaveGame);
        await _gamesUnitOfWork.UpdateSavegameStartOfLevel(dto);
        savegameListViewModel.SetBusy(false);
    }

    public async Task DeleteSavegame(SavegameListViewModel savegameListViewModel, SavegameViewModel savegameViewModel)
    {
        var userIsSure = await _messageBoxService.Show("Confirm delete".GetLocalizedString(),
            "Are you sure you want to delete this savegame? This action cannot be undone.".GetLocalizedString(),
            MsgBoxButton.YesNo,
            MsgBoxImage.Question);
        if (userIsSure.ButtonResult == MsgBoxButtonResult.No)
        {
            return;
        }

        savegameListViewModel.SetBusy("Deleting savegame...");

        await _gamesUnitOfWork.DeleteFileBackupById(savegameViewModel.Id);
        savegameListViewModel.FilteredSaves.Remove(savegameViewModel);
        savegameListViewModel.Savegames.Remove(savegameViewModel);
        savegameListViewModel.SetBusy(false);
    }

    public async Task Restore(SavegameListViewModel savegameListViewModel, int savegameId, int max)
    {
        savegameListViewModel.SetBusy("Restoring savegame...");
        var savegame = _gamesUnitOfWork.GetSavegameById(savegameId);
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
            SelectedSlot = availableSlots.FirstOrDefault(s => s.SaveSlot == savegame.SlotNumber),
            Data = savegame.Data,
            TargetDirectory = Path.GetDirectoryName(savegame.FileName),
            BaseFileName = Path.GetFileNameWithoutExtension(savegame.FileName)
        };
        _dialogService.ShowDialog(dialogViewModel, ExecuteRestore);
        savegameListViewModel.SetBusy(false);
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
        var result = await _messageBoxService.Show("Delete all savegames".GetLocalizedString(),
            "Are you sure you want to delete all savegames? This action cannot be undone.".GetLocalizedString(),
            MsgBoxButton.OkCancel, MsgBoxImage.Warning,
            checkBoxText: "Delete savegames marked as start of level".GetLocalizedString());

        if (result.ButtonResult == MsgBoxButtonResult.Ok)
        {
            savegameListViewModel.SetBusy("Deleting savegames...");
            var deleteStartOfLevel = result.CheckBoxResult;
            var targetTypes = new List<FileType>() { FileType.Savegame };
            if (deleteStartOfLevel)
            {
                targetTypes.Add(FileType.SavegameStartOfLevel);
            }

            _gamesUnitOfWork.DeleteFileBackupsByGameId(gameId, targetTypes);
            savegameListViewModel.SetBusy(false);
        }
    }

    public async Task SyncSavegames(PageViewModel page)
    {
        try
        {
            page.SetBusy("Syncing savegames...");
            var allGamesWithSaves = await _gamesUnitOfWork.GetSavegameBackups();
            
            foreach (var savegame in allGamesWithSaves)
            {
                var headerReader = _headerReaderMap[savegame.GameEngine];
                var headerData = headerReader.ReadHeader(savegame.FileName, savegame.Data);
                savegame.LevelName = headerData.LevelName;
                savegame.SlotNumber = headerData.SlotNumber;
                savegame.SaveNumber = headerData.SaveNumber;
                var md5 = await Md5Utils.ComputeMd5Hash(savegame.Data);
                if (savegame.Md5 != md5)
                {
                    savegame.Md5 = md5;
                }
            }

            await _gamesUnitOfWork.SyncSavegameMetadata(allGamesWithSaves);
            await _messageBoxService.ShowLocalized("Sync completed", "Synchronization completed successfully!", MsgBoxButton.Ok,
                MsgBoxImage.Success);
        }
        finally
        {
            page.SetBusy(false);    
        }
        
        await Task.CompletedTask;
    }

    public ISavegameHeaderReader GetHeaderReader(GameEngine gameEngine)
    {
        var headerToReturn = _headerReaderMap[gameEngine];
        _logger.LogInformation("Using savegame header reader {HeaderReaderType}", headerToReturn.GetType());
        return headerToReturn;
    }
}