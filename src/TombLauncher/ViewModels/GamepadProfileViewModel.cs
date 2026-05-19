using System.Threading.Tasks;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TombLauncher.Configuration;
using TombLauncher.Contracts.Enums;
using TombLauncher.Core.Extensions;
using TombLauncher.Localization.Extensions;
using TombLauncher.Services;

namespace TombLauncher.ViewModels;

public partial class GamepadProfileViewModel : ObservableObject
{
    private readonly ILayeredAppConfiguration _appConfiguration;
    private readonly IPopupService _popupService;

    public GamepadProfileViewModel(GameEngine engine, string? profilePath, ILayeredAppConfiguration appConfiguration, IPopupService popupService)
    {
        Engine = engine;
        ProfilePath = profilePath;
        _appConfiguration = appConfiguration;
        _popupService = popupService;
    }
    [ObservableProperty]
    public partial GameEngine Engine { get; set; }
    [ObservableProperty]
    public partial string? ProfilePath { get; set; }

    [RelayCommand]
    private async Task Browse()
    {
        var file = await _popupService.OpenFile("SELECT_GAMEPAD_PROFILE".GetLocalizedString(), [
            new FilePickerFileType("ANTIMICROX_PROFILE_FILES".GetLocalizedString())
            {
                Patterns = ["*.amgp"]
            }
        ]);

        if (file.IsNotNullOrWhiteSpace())
            ProfilePath = file;
    }

    [RelayCommand]
    private void Reset()
    {
        _appConfiguration.Defaults.Gamepad.Profiles!.TryGetValue(Engine.ToString(), out var value);
        ProfilePath = value;
    }
}