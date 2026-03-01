using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using Material.Icons;
using TombLauncher.Contracts.Enums;
using TombLauncher.Core.Extensions;
using TombLauncher.Localization.Extensions;
using TombLauncher.Services;
using TombLauncher.Utils;

namespace TombLauncher.ViewModels.Pages;

public partial class GameDetailsViewModel : PageViewModel
{
    public GameDetailsViewModel(GameDetailsService gameDetailsService)
    {
        _gameDetailsService = gameDetailsService;
        BrowseFolderCmd = new RelayCommand(BrowseFolder, CanBrowseFolder);
        ReadWalkthroughCmd = new AsyncRelayCommand<GameLinkViewModel>(ReadWalkthrough);
        ManageSaveGamesCmd = new AsyncRelayCommand(ManageSavegames);
        OpenLaunchOptionsCmd = new RelayCommand(OpenLaunchOptions);
        OpenDocumentCommand = new AsyncRelayCommand<string>(OpenDocument);
    }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ReadWalkthroughCmd))]
    private bool _askForConfirmationBeforeOpeningWalkthrough;

    [ObservableProperty] private ObservableCollection<CommandViewModel> _setupCommands = new ObservableCollection<CommandViewModel>();
    [ObservableProperty] private ObservableCollection<FileInfo> _documentationFiles = new ObservableCollection<FileInfo>();
    [ObservableProperty] private GameWithStatsViewModel _game = null!;
    [ObservableProperty] private ObservableCollection<GameLinkViewModel> _walkthroughLinks = new ObservableCollection<GameLinkViewModel>();
    public List<string> EnabledPatterns { get; set; } = new List<string>();
    public List<string> IgnoredFolders { get; set; } = new List<string>();
    private readonly GameDetailsService _gameDetailsService;

    public override async Task OnNavigatedTo(object parameter)
    {
        if (parameter is GameWithStatsViewModel game)
        {
            Game = game;
        }

        _gameDetailsService.InitializeSettings(this);
        InitSetupCommands();

        if (Game.GameMetadata.IsInstalled && Game.GameMetadata.InstallDirectory != null)
            DocumentationFiles = _gameDetailsService
                .GetDocumentationFiles(Game.GameMetadata.InstallDirectory, EnabledPatterns, IgnoredFolders)
                .ToObservableCollection();
        await _gameDetailsService.FetchLinks(this, LinkType.Walkthrough);
    }

    public ICommand BrowseFolderCmd { get; }

    private void BrowseFolder()
    {
        if (Game.GameMetadata.InstallDirectory != null)
        {
            _gameDetailsService.OpenGameFolder(Game.GameMetadata.InstallDirectory);
        }
    }

    private bool CanBrowseFolder() => Game.CanUninstall();

    public IAsyncRelayCommand ManageSaveGamesCmd { get; }

    private async Task ManageSavegames() => await _gameDetailsService.OpenSavegameList(this);

    public IRelayCommand ReadWalkthroughCmd { get; }

    private async Task ReadWalkthrough(GameLinkViewModel? link)
    {
        if (link != null)
            await _gameDetailsService.OpenWalkthrough(link.Link, AskForConfirmationBeforeOpeningWalkthrough);
    }

    [ObservableProperty] private ICommand? _installCmd;

    public ICommand OpenLaunchOptionsCmd { get; }

    private void OpenLaunchOptions() => _gameDetailsService.OpenLaunchOptions(this);

    public ICommand OpenDocumentCommand { get; }

    private async Task OpenDocument(string? path)
    {
        if (path != null)
            await _gameDetailsService.OpenWalkthrough(path, false);
    }

    private void InitSetupCommands()
    {
        var setupCommands = new ObservableCollection<CommandViewModel>();
        if (Game.GameMetadata.SetupExecutable.IsNotNullOrWhiteSpace())
        {
            setupCommands.Add(new CommandViewModel()
            {
                Command = Game.LaunchSetupCmd,
                Icon = MaterialIconKind.Settings,
                Text = "Setup".GetLocalizedString()
            });
        }

        if (Game.GameMetadata.CommunitySetupExecutable.IsNotNullOrWhiteSpace())
        {
            setupCommands.Add(new CommandViewModel()
            {
                Command = Game.LaunchCommunitySetupCmd,
                Icon = MaterialIconKind.SettingsPlay,
                Text = "Community patch setup".GetLocalizedString()
            });
        }

        SetupCommands = setupCommands;
    }
}