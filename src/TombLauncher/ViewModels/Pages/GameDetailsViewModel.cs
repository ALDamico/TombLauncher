using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using IconPacks.Avalonia.RemixIcon;
using TombLauncher.Contracts.Enums;
using TombLauncher.Core.Extensions;
using TombLauncher.Localization.Extensions;
using TombLauncher.Services;

namespace TombLauncher.ViewModels.Pages;

public partial class GameDetailsViewModel : PageViewModel
{
    public GameDetailsViewModel(GameDetailsService gameDetailsService)
    {
        _gameDetailsService = gameDetailsService;
        BrowseFolderCmd = new RelayCommand(BrowseFolder, CanBrowseFolder);
        ReadWalkthroughCmd = new AsyncRelayCommand<GameLinkViewModel>(ReadWalkthrough);
        ManageSaveGamesCmd = new AsyncRelayCommand(ManageSavegames);
        OpenLaunchOptionsCmd = new AsyncRelayCommand(OpenLaunchOptions);
        OpenDocumentCommand = new AsyncRelayCommand<string>(OpenDocument);
        _game = null!;
    }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ReadWalkthroughCmd))]
    private bool _askForConfirmationBeforeOpeningWalkthrough;

    [ObservableProperty] private ObservableCollection<CommandViewModel> _setupCommands = new ObservableCollection<CommandViewModel>();
    [ObservableProperty] private ObservableCollection<FileInfo> _documentationFiles = new ObservableCollection<FileInfo>();
    [ObservableProperty] private GameWithStatsViewModel _game;
    [ObservableProperty] private int _descriptionFontSize = 18;
    [ObservableProperty] private ObservableCollection<GameLinkViewModel> _walkthroughLinks = new ObservableCollection<GameLinkViewModel>();
    public List<string> EnabledPatterns { get; set; } = new List<string>();
    public List<string> IgnoredFolders { get; set; } = new List<string>();
    private readonly GameDetailsService _gameDetailsService;

    public override async Task OnNavigatedTo(object? parameter)
    {
        if (parameter is GameWithStatsViewModel game)
        {
            Game = game;
        }

        if (parameter == null)
        {
            Game.GameMetadata = await _gameDetailsService.GetGame(Game.GameMetadata.Id, CancellationToken.None);
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

    private async Task OpenLaunchOptions() => await _gameDetailsService.OpenLaunchOptions(this);

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
                Icon = PackIconRemixIconKind.Settings3Line,
                Text = "SETUP".GetLocalizedString()
            });
        }

        if (Game.GameMetadata.CommunitySetupExecutable.IsNotNullOrWhiteSpace())
        {
            setupCommands.Add(new CommandViewModel()
            {
                Command = Game.LaunchCommunitySetupCmd,
                Icon = PackIconRemixIconKind.PlayLargeLine,
                Text = "COMMUNITY_PATCH_SETUP".GetLocalizedString()
            });
        }

        SetupCommands = setupCommands;
    }
}