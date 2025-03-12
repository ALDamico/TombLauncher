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
using TombLauncher.Data.Database.UnitOfWork;
using TombLauncher.Extensions;
using TombLauncher.Localization.Extensions;
using TombLauncher.Navigation;
using TombLauncher.Utils;
using TombLauncher.ViewModels;
using TombLauncher.ViewModels.Dialogs;
using TombLauncher.ViewModels.MessageBoxes;
using TombLauncher.ViewModels.Pages;

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
            AppUtils.OpenUrl(link);
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

    public void OpenSavegameList(GameDetailsViewModel game)
    {
        game.SetBusy("Getting savegames...");
        var savegameListView = new SavegameListViewModel()
        {
            GameId = game.Game.GameMetadata.Id,
            GameTitle = game.Game.GameMetadata.Title,
            InstallLocation = game.Game.GameMetadata.InstallDirectory,
            GameEngine = game.Game.GameMetadata.GameEngine
        };
        game.SetBusy(false);
       
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

        var launchOptionsDto = _mapper.Map<LaunchOptionsDto>(vm);

        gameMetadata.ExecutablePath = vm.GameExecutable;
        gameMetadata.SetupExecutable = vm.SetupExecutable;
        gameMetadata.SetupExecutableArgs = vm.SetupArgs;
        gameMetadata.CommunitySetupExecutable = vm.CustomSetupExecutable;

        await GamesUnitOfWork.UpdateLaunchOptions(launchOptionsDto);
        
        NavigationManager.GetCurrentPage().ClearBusy();
    }

    public List<FileInfo> GetDocumentationFiles(string containingFolder, List<string> patterns, List<string> excludedFolders)
    {
        return patterns.Select(p => Directory.GetFiles(containingFolder, p, SearchOption.AllDirectories))
            .SelectMany(f => f)
            .Where(f => excludedFolders.All(dir => !f.Contains(dir)))
            .Select(f => new FileInfo(f))
            .ToList();
    }
}