using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
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
    }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ReadWalkthroughCommand))]
    private bool _askForConfirmationBeforeOpeningWalkthrough;

    [ObservableProperty] private ObservableCollection<CommandViewModel> _setupCommands = [];
    [ObservableProperty] private ObservableCollection<FileInfo> _documentationFiles = [];
    [ObservableProperty] private GameWithStatsViewModel _game = null!;
    [ObservableProperty] private int _descriptionFontSize = 18;
    [ObservableProperty] private ObservableCollection<GameLinkViewModel> _walkthroughLinks = [];
    public List<string> EnabledPatterns { get; set; } = [];
    public List<string> IgnoredFolders { get; set; } = [];
    private readonly GameDetailsService _gameDetailsService;

    public override async Task OnNavigatedTo(object parameter)
    {
        if (parameter is GameWithStatsViewModel game)
        {
            Game = game;
        }

        _gameDetailsService.InitializeSettings(this);
        InitSetupCommands();

        if (Game.GameMetadata is { IsInstalled: true, InstallDirectory: not null })
            DocumentationFiles = _gameDetailsService
                .GetDocumentationFiles(Game.GameMetadata.InstallDirectory, EnabledPatterns, IgnoredFolders)
                .ToObservableCollection();
        await _gameDetailsService.FetchLinks(this, LinkType.Walkthrough);
    }
    
    [RelayCommand(CanExecute = nameof(CanBrowseFolder))]
    private void BrowseFolder()
    {
        if (Game.GameMetadata.InstallDirectory != null)
        {
            _gameDetailsService.OpenGameFolder(Game.GameMetadata.InstallDirectory);
        }
    }

    private bool CanBrowseFolder() => Game.CanUninstall();

    [RelayCommand]
    private async Task ManageSavegames() => await _gameDetailsService.OpenSavegameList(this);

    [RelayCommand]
    private async Task ReadWalkthrough(GameLinkViewModel? link)
    {
        if (link != null)
            await _gameDetailsService.OpenWalkthrough(link.Link, AskForConfirmationBeforeOpeningWalkthrough);
    }

    [ObservableProperty] private ICommand? _installCmd;

    [RelayCommand]
    private void OpenLaunchOptions() => _gameDetailsService.OpenLaunchOptions(this);

    [RelayCommand]
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
                Command = Game.LaunchSetupCommand,
                Icon = PackIconRemixIconKind.Settings3Line,
                Text = "SETUP".GetLocalizedString()
            });
        }

        if (Game.GameMetadata.CommunitySetupExecutable.IsNotNullOrWhiteSpace())
        {
            setupCommands.Add(new CommandViewModel()
            {
                Command = Game.LaunchCommunitySetupCommand,
                Icon = PackIconRemixIconKind.PlayLargeLine,
                Text = "COMMUNITY_PATCH_SETUP".GetLocalizedString()
            });
        }

        SetupCommands = setupCommands;
    }
}