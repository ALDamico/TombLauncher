using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using JamSoft.AvaloniaUI.Dialogs;
using JamSoft.AvaloniaUI.Dialogs.MsgBox;
using TombLauncher.Contracts.Enums;
using TombLauncher.Contracts.Localization;
using TombLauncher.Core.Dtos;
using TombLauncher.Core.Extensions;
using TombLauncher.Core.Savegames;
using TombLauncher.Data.Database.UnitOfWork;
using TombLauncher.Extensions;
using TombLauncher.Localization.Extensions;
using TombLauncher.Navigation;
using TombLauncher.ViewModels;
using TombLauncher.ViewModels.Dialogs;
using TombLauncher.ViewModels.MessageBoxes;
using TombLauncher.ViewModels.Pages;
using Path = Avalonia.Controls.Shapes.Path;

namespace TombLauncher.Services;

public class GameDetailsService : IViewService
{
    public GameDetailsService(GamesUnitOfWork gamesUnitOfWork, ILocalizationManager localizationManager,
        NavigationManager navigationManager, IMessageBoxService messageBoxService, IDialogService dialogService, MapperConfiguration mapperConfiguration)
    {
        GamesUnitOfWork = gamesUnitOfWork;
        LocalizationManager = localizationManager;
        NavigationManager = navigationManager;
        MessageBoxService = messageBoxService;
        DialogService = dialogService;
        _mapper = mapperConfiguration.CreateMapper();
    }

    public GamesUnitOfWork GamesUnitOfWork { get; set; }
    public ILocalizationManager LocalizationManager { get; set; }
    public NavigationManager NavigationManager { get; set; }
    public IMessageBoxService MessageBoxService { get; }
    public IDialogService DialogService { get; }
    private IMapper _mapper;

    public void OpenGameFolder(string gameFolder)
    {
        Process.Start("explorer", gameFolder);
    }

    public bool CanUninstall(GameMetadataViewModel metadataViewModel)
    {
        return metadataViewModel.IsInstalled;
    }

    public async Task Uninstall(string installDir, int gameId)
    {
        NavigationManager.GetCurrentPage().SetBusy("Uninstalling...");
        Directory.Delete(installDir, true);
        GamesUnitOfWork.MarkGameAsUninstalled(gameId);
        await GamesUnitOfWork.Save();
        NavigationManager.GoBack();
    }

    public async Task FetchLinks(GameDetailsViewModel game, LinkType linkType)
    {
        var tf = new TaskFactory();
        var links = await tf.StartNew(() =>
        {
            var links = GamesUnitOfWork.GetLinks(game.Game.GameMetadata.Id, linkType);
            return _mapper.Map<List<GameLinkViewModel>>(links);
        });
        game.WalkthroughLinks = links.ToObservableCollection();
    }

    public async Task OpenWalkthrough(string link, bool askConfirmation)
    {
        if (askConfirmation)
        {
            var confirmation = await MessageBoxService.ShowLocalized("Confirm",
                "Are you sure you want to read the walkthrough?",
                 MsgBoxButton.YesNo, MsgBoxImage.Question);
            if (confirmation.ButtonResult == MsgBoxButtonResult.No)
                return;
        }
        
        try
        {
            link = link.Replace("&", "^&");
            Process.Start(new ProcessStartInfo(link) { UseShellExecute = true });
        }
        catch (SystemException)
        {
            await MessageBoxService.Show(new UrlOpenErrorMessageBox()
            {
                MsgBoxImage = MsgBoxImage.Error,
                Message =
                    "An error occurred while trying to open the walkthrough at WALKTHROUGH_URL. Would you like to copy this URL to the clipboard?".GetLocalizedString(link),
                MsgBoxTitle = "Error".GetLocalizedString(),
                TargetUrl = link
            });
        }
    }

    public async Task OpenSavegameList(GameDetailsViewModel game)
    {
        game.SetBusy("Getting savegames...");
        var savegameListView = new SavegameListViewModel()
            { GameId = game.Game.GameMetadata.Id, GameTitle = game.Game.GameMetadata.Title , InstallLocation = game.Game.GameMetadata.InstallDirectory};
        game.SetBusy(false);
        /*
        

        if (savegameListView.Savegames.Count == 0)
        {
            var installDir = game.Game.GameMetadata.InstallDirectory;

            var savegames = Directory.GetFiles(installDir, "save*.*", SearchOption.AllDirectories).Where(f => System.IO.Path.GetExtension(f).TrimStart('.').All(char.IsDigit)).ToList();

            var userResponse = await MessageBoxService.Show("No savegame backups found",
                $"There were no savegame backups in Tomb Launcher's database, but we found {savegames.Count} savegame files. Would you like to import them?",
                MsgBoxButton.YesNo, MsgBoxImage.Folder, "No".GetLocalizedString(), "Yes".GetLocalizedString());
            if (userResponse.ButtonResult == MsgBoxButtonResult.Yes)
            {
                game.SetBusy("Importing your saved games...");
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

                GamesUnitOfWork.BackupSavegames(game.Game.GameMetadata.Id, dataToBackup);
                game.SetBusy(false);
            }
            else
            {
                game.SetBusy(false);
                return;
            }
            
            
        }*/
        NavigationManager.NavigateTo(savegameListView);
    }

    public void OpenLaunchOptions(GameDetailsViewModel gameDetailsViewModel)
    {
        DialogService.ShowDialog(new LaunchOptionsDialogViewModel(){TargetGame = gameDetailsViewModel.Game.GameMetadata}, SaveLaunchOptions);
    }

    private async void SaveLaunchOptions(LaunchOptionsDialogViewModel vm)
    {
        var gameMetadata = vm.TargetGame;
        NavigationManager.GetCurrentPage().SetBusy("Saving launch options...");

        var launchOptionsDto = new LaunchOptionsDto()
        {
            GameEngine = vm.SelectedEngine,
            GameId = gameMetadata.Id,
            GameExecutable = new FileBackupDto()
            {
                FileName = vm.GameExecutable,
                GameId = gameMetadata.Id,
                FileType = FileType.GameExecutable
            }
        };
        if (vm.SupportsSetup)
        {
            launchOptionsDto.SetupExecutable = new FileBackupDto()
            {
                Arguments = vm.SetupArgs,
                FileName = vm.SetupExecutable,
                FileType = FileType.SetupExecutable,
                GameId = gameMetadata.Id
            };
        }

        if (vm.SupportsCustomSetup)
        {
            launchOptionsDto.CommunitySetupExecutable = new FileBackupDto()
            {
                FileName = vm.CustomSetupExecutable,
                FileType = FileType.CommunitySetupExecutable,
                GameId = gameMetadata.Id
            };
        }

        await GamesUnitOfWork.UpdateLaunchOptions(launchOptionsDto);
        
        NavigationManager.GetCurrentPage().ClearBusy();
        
    }
}