using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TombLauncher.Contracts.Enums;
using TombLauncher.Localization.Extensions;
using TombLauncher.Patchers.Widescreen;

namespace TombLauncher.ViewModels.Pages;

public partial class WidescreenPatcherViewModel : PageViewModel
{
    [ObservableProperty] private bool _updateAspectRatio;
    [ObservableProperty] private float _aspectRatioWidth = 16;
    [ObservableProperty] private float _aspectRatioHeight = 9;

    [ObservableProperty] private bool _updateCameraDistance;
    [ObservableProperty] private CameraDistanceOptionViewModel _selectedCameraDistanceOption;
    [ObservableProperty] private short _customCameraDistance = (short)CameraDistancePreset.OneAndAHalf;

    [ObservableProperty] private bool _updateFov;
    [ObservableProperty] private int _targetFov = 1920;

    [ObservableProperty] private bool _update60Fps;
    [ObservableProperty] private bool _is60FpsAvailable;
    [ObservableProperty] private string? _fps60TooltipText;

    public ObservableCollection<CameraDistanceOptionViewModel> CameraDistanceOptions { get; }

    private GameMetadataViewModel? _gameMetadata;
    private readonly WidescreenPatcherService _patcherService;

    public WidescreenPatcherViewModel(WidescreenPatcherService patcherService)
    {
        _patcherService = patcherService;

        var oneAndAHalfPreset = new CameraDistanceOptionViewModel
        {
            Value = CameraDistancePreset.OneAndAHalf,
            DisplayText = "CAMERA_DISTANCE_PRESET_ONE_AND_HALF_BLOCKS".GetLocalizedString()
        };

        CameraDistanceOptions =
        [
            new CameraDistanceOptionViewModel { Value = CameraDistancePreset.One,           DisplayText = "CAMERA_DISTANCE_PRESET_ONE_BLOCK".GetLocalizedString() },
            oneAndAHalfPreset,
            new CameraDistanceOptionViewModel { Value = CameraDistancePreset.Two,           DisplayText = "CAMERA_DISTANCE_PRESET_TWO_BLOCKS".GetLocalizedString() },
            new CameraDistanceOptionViewModel { Value = CameraDistancePreset.TwoAndAHalf,   DisplayText = "CAMERA_DISTANCE_PRESET_TWO_AND_HALF_BLOCKS".GetLocalizedString() },
            new CameraDistanceOptionViewModel { Value = CameraDistancePreset.Three,         DisplayText = "CAMERA_DISTANCE_PRESET_THREE_BLOCKS".GetLocalizedString() },
            new CameraDistanceOptionViewModel { Value = CameraDistancePreset.Custom,        DisplayText = "CAMERA_DISTANCE_PRESET_CUSTOM".GetLocalizedString() },
        ];

        SelectedCameraDistanceOption = oneAndAHalfPreset; // OneAndAHalf
    }

    public override Task OnNavigatedTo(object parameter)
    {
        _gameMetadata = (GameMetadataViewModel)parameter;
        Is60FpsAvailable = _patcherService.Check60FpsSupport(_gameMetadata.InstallDirectory ?? "");
        if (!Is60FpsAvailable)
            Fps60TooltipText = string.Format(
                "FPS_60_NOT_SUPPORTED".GetLocalizedString(),
                _gameMetadata.GameEngine);

        return Task.CompletedTask;
    }

    [RelayCommand]
    private async Task ApplyPatch()
    {
    }
}
