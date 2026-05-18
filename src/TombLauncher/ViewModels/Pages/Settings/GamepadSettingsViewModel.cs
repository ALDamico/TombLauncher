using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using IconPacks.Avalonia.RemixIcon;
using TombLauncher.Configuration;
using TombLauncher.Contracts.Enums;
using TombLauncher.Core.Extensions;
using TombLauncher.Core.PlatformSpecific;
using TombLauncher.Core.Utils;
using TombLauncher.Gamepad;
using TombLauncher.Services;

namespace TombLauncher.ViewModels.Pages.Settings;

public partial class GamepadSettingsViewModel : SettingsSectionViewModelBase
{
    private readonly IPlatformSpecificFeatures _platformSpecificFeatures;

    public GamepadSettingsViewModel(PageViewModel settingsPage, ILayeredAppConfiguration appConfiguration,
        ViewServiceContext viewServiceContext, IPlatformSpecificFeatures platformSpecificFeatures) : base("GAMEPAD", settingsPage,
        PackIconRemixIconKind.GamepadLine)
    {
        _platformSpecificFeatures = platformSpecificFeatures;
        Profiles = appConfiguration.Gamepad.Profiles!.Select(p =>
                new GamepadProfileViewModel(Enum.Parse<GameEngine>(p.Key), p.Value, appConfiguration,
                    viewServiceContext))
            .ToObservableCollection();
    }

    [ObservableProperty] public partial GamepadTool GamepadTool { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsToolValid))]
    public partial string? GamepadToolPath { get; set; }

    public ObservableCollection<GamepadProfileViewModel> Profiles { get; }

    public bool IsToolValid => GamepadToolPath.IsNotNullOrWhiteSpace() && PathUtils.ExistsOnPath(GamepadToolPath) &&
                               _platformSpecificFeatures.IsExecutable(GamepadToolPath!);

    public override void ApplyTo(AppConfiguration userConfig)
    {
        userConfig.Gamepad.GamepadTool = GamepadTool;
        userConfig.Gamepad.Profiles = Profiles
            .Select(p => new KeyValuePair<string, string?>(p.Engine.ToString(), p.ProfilePath)).ToDictionary();
        userConfig.Gamepad.ToolPath = GamepadToolPath;
        base.ApplyTo(userConfig);
    }
}