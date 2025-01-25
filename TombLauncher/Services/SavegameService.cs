using System;
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
using TombLauncher.Contracts.Enums;
using TombLauncher.Core.Dtos;
using TombLauncher.Core.Extensions;
using TombLauncher.Core.Savegames;
using TombLauncher.Core.Utils;
using TombLauncher.Data.Database.UnitOfWork;
using TombLauncher.Localization.Extensions;
using TombLauncher.ViewModels;
using TombLauncher.ViewModels.Pages;
using Path = System.IO.Path;

namespace TombLauncher.Services;

public class SavegameService
{
    public SavegameService()
    {
        _gamesUnitOfWork = Ioc.Default.GetRequiredService<GamesUnitOfWork>();
        _messageBoxService = Ioc.Default.GetRequiredService<IMessageBoxService>();
        _mapper = Ioc.Default.GetRequiredService<MapperConfiguration>().CreateMapper();
    }

    private readonly GamesUnitOfWork _gamesUnitOfWork;
    private readonly IMessageBoxService _messageBoxService;
    private readonly IMapper _mapper;

    public Task InitGameTitle(SavegameListViewModel targetViewModel)
    {
        targetViewModel.SetBusy(true);
        var gameTitle = _gamesUnitOfWork.GetGameById(targetViewModel.GameId);
        targetViewModel.GameTitle = gameTitle.Title;
        return Task.CompletedTask;
    }

    public async Task LoadSaveGames(SavegameListViewModel targetViewModel)
    {
        targetViewModel.SetBusy("Fetching savegames for GAMETITLE".GetLocalizedString(targetViewModel.GameTitle));
        var observableCollection = new ObservableCollection<SavegameViewModel>();
        var knownSavegames =
            await Task.Factory.StartNew(() => _gamesUnitOfWork.GetSavegamesByGameId(targetViewModel.GameId));
        var headerParser = new SavegameHeaderReader();
        foreach (var savegame in knownSavegames)
        {
            var savegameHeader = headerParser.ReadHeader(savegame.FileName, savegame.Data);
            var viewModel = new SavegameViewModel()
            {
                Id = savegame.Id,
                Filename = savegame.FileName,
                LevelName = savegameHeader.LevelName,
                SaveNumber = savegameHeader.SaveNumber,
                IsStartOfLevel = savegame.FileType == FileType.SavegameStartOfLevel,
                SlotNumber = int.Parse(Path.GetExtension(savegame.FileName).TrimStart('.')) + 1,
                UpdateStartOfLevelStateCmd = targetViewModel.UpdateStartOfLevelStateCmd
            };
            observableCollection.Add(viewModel);
        }

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
            Header = "All slots",
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
            Header = $"Slot #{s}",
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
            .ToObservableCollection();
        savegameListViewModel.FilteredSaves = savegamesBySlot;
        savegameListViewModel.SetBusy(false);
        return Task.CompletedTask;
    }

    public async Task CheckSavegamesNotBackedUp(SavegameListViewModel savegameListView)
    {
        if (savegameListView.Savegames.Count == 0)
        {
            var installDir = savegameListView.InstallLocation;

            var savegames = Directory.GetFiles(installDir, "save*.*", SearchOption.AllDirectories)
                .Where(f => Path.GetExtension(f).TrimStart('.').All(char.IsDigit))
                .ToList();
            var existingGamesDict = new Dictionary<string, string>();
            foreach (var savegameFile in savegames)
            {
                var fileContent = await File.ReadAllBytesAsync(savegameFile);
                var md5 = await Md5Utils.ComputeMd5Hash(fileContent);
                existingGamesDict[md5] = savegameFile;
            }

            var backedUpSaves = _gamesUnitOfWork.GetSavegameMd5sByGameId(savegameListView.GameId);
            var missingSaveGames = existingGamesDict.Keys.Except(backedUpSaves).Intersect(existingGamesDict.Keys);

            var userResponse = await _messageBoxService.Show("No savegame backups found",
                $"There were no savegame backups in Tomb Launcher's database, but we found {savegames.Count} savegame files. Would you like to import them?",
                MsgBoxButton.YesNo, MsgBoxImage.Folder, "No".GetLocalizedString(), "Yes".GetLocalizedString());
            if (userResponse.ButtonResult == MsgBoxButtonResult.Yes)
            {
                savegameListView.SetBusy("Importing your saved games...");
                var headerReader = new SavegameHeaderReader();
                var dataToBackup = new List<FileBackupDto>();
                foreach (var file in savegames)
                {
                    var data = headerReader.ReadHeader(file);
                    if (data != null)
                    {
                        var dto = new FileBackupDto()
                        {
                            Data = File.ReadAllBytes(file),
                            FileName = file,
                            FileType = FileType.Savegame,
                            BackedUpOn = DateTime.Now
                        };
                        dataToBackup.Add(dto);
                    }
                }

                _gamesUnitOfWork.BackupSavegames(savegameListView.GameId, dataToBackup);
                _gamesUnitOfWork.Save();
                savegameListView.SetBusy(false);
            }
            else
            {
                savegameListView.SetBusy(false);
                return;
            }
        }
    }

    public async Task UpdateStartOfLevelState(SavegameListViewModel savegameListViewModel,
        SavegameViewModel targetSaveGame)
    {
        savegameListViewModel.SetBusy("Update in progress...");
        var dto = _mapper.Map<FileBackupDto>(targetSaveGame);
        _gamesUnitOfWork.UpdateSavegameStartOfLevel(dto);
        savegameListViewModel.SetBusy(false);
    }
}