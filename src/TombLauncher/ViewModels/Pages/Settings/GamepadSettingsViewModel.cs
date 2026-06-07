using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using IconPacks.Avalonia.RemixIcon;
using TombLauncher.Configuration;
using TombLauncher.Contracts.Enums;
using TombLauncher.Contracts.PlatformSpecific;
using TombLauncher.Core.Extensions;
using TombLauncher.Core.PlatformSpecific;
using TombLauncher.Core.Utils;
using TombLauncher.Gamepad;
using TombLauncher.Localization.Extensions;
using TombLauncher.Services;

namespace TombLauncher.ViewModels.Pages.Settings;

public partial class GamepadSettingsViewModel : SettingsSectionViewModelBase
{
    private readonly IPlatformSpecificFeatures _platformSpecificFeatures;

    public GamepadSettingsViewModel(PageViewModel settingsPage, ILayeredAppConfiguration appConfiguration,
        IPopupService viewServiceContext, IPlatformSpecificFeatures platformSpecificFeatures) : base("GAMEPAD", settingsPage,
        PackIconRemixIconKind.GamepadLine)
    {
        _platformSpecificFeatures = platformSpecificFeatures;
        Profiles = appConfiguration.Gamepad.Profiles!.Select(p =>
                new GamepadProfileViewModel(Enum.Parse<GameEngine>(p.Key), p.Value, appConfiguration,
                    viewServiceContext))
            .ToObservableCollection();
        AvailableGamepadTools = new ObservableCollection<EnumViewModel<GamepadTool>>([
            new EnumViewModel<GamepadTool>(GamepadTool.None),
            new EnumViewModel<GamepadTool>(GamepadTool.None) { IsDisabled = true },
            new EnumViewModel<GamepadTool>(GamepadTool.AntiMicroX)
        ]);
        IsToolValid = new ServiceCheckViewModel() { Status = ServiceCheckStatus.Unspecified, CheckResultMessage = "" };
    }

    [ObservableProperty] public partial GamepadTool GamepadTool { get; set; }

    [ObservableProperty]
    public partial string? GamepadToolPath { get; set; }

    public ObservableCollection<GamepadProfileViewModel> Profiles { get; }
    public ObservableCollection<EnumViewModel<GamepadTool>> AvailableGamepadTools { get; }

    partial void OnGamepadToolPathChanged(string? value)
    {
        var isValid = GamepadToolPath.IsNotNullOrWhiteSpace() && PathUtils.ExistsOnPath(GamepadToolPath) &&
                      _platformSpecificFeatures.IsExecutable(GamepadToolPath!);

        IsToolValid = new ServiceCheckViewModel()
        {
            Status = isValid ? ServiceCheckStatus.Okay : ServiceCheckStatus.Error,
            CheckResultMessage = isValid ? "GAMEPAD_TOOL_EXISTS".GetLocalizedString() : "GAMEPAD_TOOL_NOT_VALID".GetLocalizedString()
        };
    }

    [ObservableProperty]
    public partial ServiceCheckViewModel IsToolValid { get; set; }

    public override void ApplyTo(AppConfiguration userConfig)
    {
        userConfig.Gamepad.GamepadTool = GamepadTool;
        userConfig.Gamepad.Profiles = Profiles
            .Select(p => new KeyValuePair<string, string?>(p.Engine.ToString(), p.ProfilePath)).ToDictionary();
        userConfig.Gamepad.ToolPath = GamepadToolPath;
        base.ApplyTo(userConfig);
    }
}