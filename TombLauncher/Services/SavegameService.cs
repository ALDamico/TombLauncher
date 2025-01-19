using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Avalonia.Controls.Shapes;
using CommunityToolkit.Mvvm.DependencyInjection;
using TombLauncher.Contracts.Enums;
using TombLauncher.Core.Savegames;
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
    }

    private readonly GamesUnitOfWork _gamesUnitOfWork;

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
        var knownSavegames = await Task.Factory.StartNew(() => _gamesUnitOfWork.GetSavegamesByGameId(targetViewModel.GameId));
        var headerParser = new SavegameHeaderReader();
        foreach (var savegame in knownSavegames)
        {
            var savegameHeader = headerParser.ReadHeader(savegame.FileName, savegame.Data);
            var viewModel = new SavegameViewModel()
            {
                Filename = savegame.FileName,
                LevelName = savegameHeader.LevelName,
                SaveNumber = savegameHeader.SaveNumber,
                IsStartOfLevel = savegame.FileType == FileType.SavegameStartOfLevel,
                SlotNumber = int.Parse(Path.GetExtension(savegame.FileName).TrimStart('.')) + 1
            };
            observableCollection.Add(viewModel);
        }

        targetViewModel.Savegames = observableCollection;
    }
}