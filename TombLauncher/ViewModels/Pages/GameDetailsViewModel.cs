using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using JamSoft.AvaloniaUI.Dialogs;
using Material.Icons;
using TombLauncher.Contracts.Enums;
using TombLauncher.Core.Extensions;
using TombLauncher.Localization.Extensions;
using TombLauncher.Services;

namespace TombLauncher.ViewModels.Pages;

public partial class GameDetailsViewModel : PageViewModel
{
    public GameDetailsViewModel(GameWithStatsViewModel game) 
    {
        _game = game;
        _gameDetailsService = Ioc.Default.GetRequiredService<GameDetailsService>();
        BrowseFolderCmd = new RelayCommand(BrowseFolder, CanBrowseFolder);
        UninstallCmd = new AsyncRelayCommand(Uninstall, CanUninstall);
        ReadWalkthroughCmd = new AsyncRelayCommand<GameLinkViewModel>(ReadWalkthrough);
        ManageSaveGamesCmd = new RelayCommand(ManageSavegames);
        OpenLaunchOptionsCmd = new RelayCommand(OpenLaunchOptions);
    }

    private bool _askForConfirmationBeforeOpeningWalkthrough;
    private bool _useInternalViewerIfAvailable;
    //private IDialogService _dialogService;
    [ObservableProperty] private ObservableCollection<CommandViewModel> _setupCommands;

    protected override async Task RaiseInitialize()
    {
        var settingsService = Ioc.Default.GetRequiredService<SettingsService>();
        var gameDetailsSettings = settingsService.GetGameDetailsSettings();
        _askForConfirmationBeforeOpeningWalkthrough = gameDetailsSettings.AskForConfirmationBeforeWalkthrough;
        _useInternalViewerIfAvailable = gameDetailsSettings.UseInternalViewerIfAvailable;
        SetupCommands = new ObservableCollection<CommandViewModel>();
        if (Game.GameMetadata.SetupExecutable.IsNotNullOrWhiteSpace())
        {
            SetupCommands.Add(new CommandViewModel(){Command = Game.LaunchSetupCmd, Icon = MaterialIconKind.Settings, Text = "Setup".GetLocalizedString()});
        }

        if (Game.GameMetadata.CommunitySetupExecutable.IsNotNullOrWhiteSpace())
        {
            SetupCommands.Add(new CommandViewModel(){Command = Game.LaunchCommunitySetupCmd, Icon = MaterialIconKind.SettingsPlay, Text = "Community patch setup".GetLocalizedString()});
        }
        await _gameDetailsService.FetchLinks(this, LinkType.Walkthrough);
    }

    private readonly GameDetailsService _gameDetailsService;
    [ObservableProperty] private GameWithStatsViewModel _game;
    [ObservableProperty] private ObservableCollection<GameLinkViewModel> _walkthroughLinks;
    public ICommand BrowseFolderCmd { get; }

    private void BrowseFolder()
    {
        _gameDetailsService.OpenGameFolder(Game.GameMetadata.InstallDirectory);
    }

    private bool CanBrowseFolder()
    {
        return _gameDetailsService.CanUninstall(Game.GameMetadata);
    }
    
    public ICommand ManageSaveGamesCmd { get; }

    private void ManageSavegames()
    {
        _gameDetailsService.OpenSavegameList(this);
    }
    
    public ICommand ReadWalkthroughCmd { get; }

    private async Task ReadWalkthrough(GameLinkViewModel link)
    {
        await _gameDetailsService.OpenWalkthrough(link.Link, _askForConfirmationBeforeOpeningWalkthrough);
    }
    
    public ICommand UninstallCmd { get; }

    private async Task Uninstall()
    {
        await _gameDetailsService.Uninstall(Game.GameMetadata.InstallDirectory, Game.GameMetadata.Id);
    }

    private bool CanUninstall()
    {
        return _gameDetailsService.CanUninstall(Game.GameMetadata);
    }

    [ObservableProperty] private ICommand _installCmd;
    
    public ICommand OpenLaunchOptionsCmd { get; }

    private void OpenLaunchOptions()
    {
        _gameDetailsService.OpenLaunchOptions(this);
    }
}