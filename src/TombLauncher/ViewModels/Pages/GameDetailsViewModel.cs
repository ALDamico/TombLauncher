using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EnumsNET;
using IconPacks.Avalonia.RemixIcon;
using TombLauncher.Contracts.Enums;
using TombLauncher.Contracts.PlatformSpecific;
using TombLauncher.Core.Extensions;
using TombLauncher.Core.PlatformSpecific;
using TombLauncher.Localization.Extensions;
using TombLauncher.Services;
using TombLauncher.Utils;

namespace TombLauncher.ViewModels.Pages;

public partial class GameDetailsViewModel : PageViewModel
{
    public GameDetailsViewModel(GameDetailsService gameDetailsService, IPlatformSpecificFeatures platformSpecificFeatures)
    {
        _gameDetailsService = gameDetailsService;
        _platformSpecificFeatures = platformSpecificFeatures;
    }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ReadWalkthroughCommand))]
    public partial bool AskForConfirmationBeforeOpeningWalkthrough { get; set; }
    [ObservableProperty]
    public partial ObservableCollection<CommandViewModel> SetupCommands { get; set; } = [];
    [ObservableProperty]
    public partial ObservableCollection<FileInfo> DocumentationFiles { get; set; } = [];

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanOpenChat))]
    [NotifyPropertyChangedFor(nameof(EngineSupportState))]
    [NotifyPropertyChangedFor(nameof(PlayButtonTooltip))]
    [NotifyPropertyChangedFor(nameof(NativeGamepadSupport))]
    private GameWithStatsViewModel _game = null!;

    [ObservableProperty] private int _descriptionFontSize = 18;
    [ObservableProperty]
    public partial ObservableCollection<GameLinkViewModel> WalkthroughLinks { get; set; } = [];
    [ObservableProperty]
    public partial ObservableCollection<CommandViewModel> Patchers { get; set; } = [];

    public bool CanOpenChat => _gameDetailsService.CanOpenChat(Game?.GameMetadata);
    public List<string> EnabledPatterns { get; set; } = [];
    public List<string> IgnoredFolders { get; set; } = [];
    private readonly GameDetailsService _gameDetailsService;
    private readonly IPlatformSpecificFeatures _platformSpecificFeatures;

    public string PlayButtonTooltip
    {
        get
        {
            if (Game?.GameMetadata.GameEngine == GameEngine.Unknown)
            {
                return "AUTOMATIC_ENGINE_DETECTION_FAILED_SET_MANUALLY".GetLocalizedString();
            }

            return "PLAY".GetLocalizedString();
        }
    }

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
        InitPatchers();

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

    [ObservableProperty] private ICommand? _installCommand;

    [RelayCommand]
    private async Task OpenLaunchOptions() => await _gameDetailsService.OpenLaunchOptions(this);

    [RelayCommand]
    private async Task OpenDocument(string? path)
    {
        if (path != null)
            await _gameDetailsService.OpenWalkthrough(path, false);
    }

    [RelayCommand]
    private async Task OpenChat() => await _gameDetailsService.OpenChat(Game.GameMetadata.Id, CancellationToken.None);

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

    private void InitPatchers()
    {
        var patchers = new ObservableCollection<CommandViewModel>();

        if (Game.GameMetadata.GameEngine is GameEngine.TombRaider2 or GameEngine.TombRaider3 or GameEngine.TombRaider4
            or GameEngine.TombRaider5)
        {
            patchers.Add(new CommandViewModel()
            {
                Command = OpenWidescreenPatcherCommand, 
                Text = "WIDESCREEN_PATCH".GetLocalizedString(), 
                Icon = PackIconRemixIconKind.AspectRatioLine
            });
        }

        var trxEngines = new List<GameEngine>() { GameEngine.Tr1x, GameEngine.Tr2x, GameEngine.Trx };

        if (_platformSpecificFeatures.Platform == Platform.Linux && trxEngines.Contains(Game.GameMetadata.GameEngine))
        {
            patchers.Add(new CommandViewModel()
            {
                Command = new AsyncRelayCommand(() => _gameDetailsService.OpenTrxNativePatcher(Game.GameMetadata)),
                Text = "CONVERT_TO_NATIVE_EXECUTABLE".GetLocalizedString(),
                Icon = PackIconRemixIconKind.UbuntuLine
            });
        }

        if (Game.GameMetadata.GameEngine.GetBaseEngine().HasAnyFlags(EnumUtils.ClassicEngines))
        {
            patchers.Add(new CommandViewModel()
            {
                Command = new AsyncRelayCommand(() => { Console.WriteLine("Implement me!");
                    return Task.CompletedTask;
                }), // TODO
                Icon = PackIconRemixIconKind.Window2Fill,
                Text = "ENABLE_FULLSCREEN_BORDER_FIX".GetLocalizedString(),
                IsCheckable = true
            });
        }

        Patchers = patchers;
    }

    [RelayCommand]
    private async Task OpenWidescreenPatcher() =>
        await _gameDetailsService.OpenWidescreenPatcher(Game.GameMetadata);

    public EngineSupportState EngineSupportState => _gameDetailsService.GetEngineSupportState(Game?.GameMetadata);
    public bool NativeGamepadSupport => _gameDetailsService.GetGamepadSupport(Game?.GameMetadata);
}