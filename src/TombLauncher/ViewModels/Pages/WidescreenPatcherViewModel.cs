using System;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using JamSoft.AvaloniaUI.Dialogs.MsgBox;
using TombLauncher.Contracts.Enums;
using TombLauncher.Extensions;
using TombLauncher.Localization.Extensions;
using TombLauncher.Patchers.Widescreen;
using TombLauncher.Services.Patchers.Widescreen;

namespace TombLauncher.ViewModels.Pages;

public partial class WidescreenPatcherViewModel : PageViewModel
{
    [ObservableProperty][NotifyPropertyChangedFor(nameof(CanApplyPatchByFormState))][NotifyCanExecuteChangedFor(nameof(ApplyPatchCommand))] private bool _updateAspectRatio;
    [ObservableProperty] private float _aspectRatioWidth = 16;
    [ObservableProperty] private float _aspectRatioHeight = 9;

    [ObservableProperty][NotifyPropertyChangedFor(nameof(CanApplyPatchByFormState))][NotifyCanExecuteChangedFor(nameof(ApplyPatchCommand))] private bool _updateCameraDistance;
    [ObservableProperty] private CameraDistanceOptionViewModel _selectedCameraDistanceOption;
    [ObservableProperty] private short _customCameraDistance = (short)CameraDistancePreset.OneAndAHalf;

    [ObservableProperty][NotifyPropertyChangedFor(nameof(CanApplyPatchByFormState))][NotifyCanExecuteChangedFor(nameof(ApplyPatchCommand))] private bool _updateFov;
    [ObservableProperty] private int _targetFov = 1920;

    [ObservableProperty][NotifyPropertyChangedFor(nameof(CanApplyPatchByFormState))][NotifyCanExecuteChangedFor(nameof(ApplyPatchCommand))] private bool _update60Fps;
    [ObservableProperty] private bool _is60FpsAvailable;
    [ObservableProperty] private string? _fps60TooltipText;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanRevertPatch))]
    [NotifyPropertyChangedFor(nameof(CanApplyPatch))]
    [NotifyCanExecuteChangedFor(nameof(ApplyPatchCommand), nameof(RevertPatchCommand))]
    private bool _canApplyPatchByFileState;
    public bool CanRevertPatch => !CanApplyPatchByFileState;

    public bool CanApplyPatch => CanApplyPatchByFileState && CanApplyPatchByFormState;
    
    public bool CanApplyPatchByFormState => Update60Fps || UpdateAspectRatio || UpdateFov || UpdateCameraDistance;

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

    public override async Task OnNavigatedTo(object parameter)
    {
        _gameMetadata = (GameMetadataViewModel)parameter;
        using (BusyScope("CHECKING_60_FPS_SUPPORT".GetLocalizedString()))
        {
            Is60FpsAvailable = _patcherService.Check60FpsSupport(_gameMetadata.InstallDirectory ?? "");
            if (!Is60FpsAvailable)
                Fps60TooltipText = "FPS_60_NOT_SUPPORTED".GetLocalizedString(_gameMetadata.GameEngine.GetDescription());

            var progress = GetProgress();
            var isPatchApplied = await _patcherService.IsAlreadyApplied(_gameMetadata.Id, progress, CancellationToken.None);
            if (!isPatchApplied.IsSuccessful)
            {
                CanApplyPatchByFileState = true;
            }
        }
    }

    private IProgress<string> GetProgress() => new Progress<string>(msg => BusyMessage = msg);

    private int GetCameraDistance()
    {
        if (SelectedCameraDistanceOption.IsCustom)
            return CustomCameraDistance;

        return (int)SelectedCameraDistanceOption.Value;
    }

    [RelayCommand(CanExecute = nameof(CanApplyPatch))]
    private async Task ApplyPatch()
    {
        var parameters = new WidescreenPatcherParameters()
        {
            Engine = _gameMetadata!.GameEngine,
            Update60Fps = Update60Fps,
            UpdateAspectRatio = UpdateAspectRatio,
            UpdateCameraDistance = UpdateCameraDistance,
            UpdateFov = UpdateFov,
            TargetAspectRatio = AspectRatioWidth / AspectRatioHeight,
            TargetCameraDistance = GetCameraDistance(),
            TargetFov = TargetFov
        };

        using (BusyScope("APPLYING_WIDESCREEN_PATCH".GetLocalizedString()))
        {
            var progress = GetProgress();
            var patchResult = await _patcherService.ApplyPatch(_gameMetadata, parameters, progress, CancellationToken.None);

            if (patchResult.IsSuccessful)
            {
                CanApplyPatchByFileState = false;
                await _patcherService.ViewContext.PopupService.ShowLocalized(
                    "WIDESCREEN_PATCH_SUCCESSFULLY_APPLIED".GetLocalizedString(),
                    "WIDESCREEN_PATCH_APPLIED_TITLE".GetLocalizedString(), MsgBoxButton.Ok, MsgBoxImage.Information);
            }
            else
            {
                await _patcherService.ViewContext.PopupService.ShowLocalized(patchResult.Message,
                    "WIDESCREEN_PATCH_ERROR_TITLE".GetLocalizedString(), MsgBoxButton.Ok, MsgBoxImage.Error);
            }
        }
    }

    [RelayCommand(CanExecute = nameof(CanRevertPatch))]
    private async Task RevertPatch()
    {
        using (BusyScope("REVERTING_WIDESCREEN_PATCH".GetLocalizedString()))
        {
            var progress = GetProgress();
            var patchReversalResult =
                await _patcherService.RevertPatch(_gameMetadata!, progress, CancellationToken.None);
            if (patchReversalResult.IsSuccessful)
            {
                CanApplyPatchByFileState = true;
                await _patcherService.ViewContext.PopupService.ShowLocalized(
                    "WIDESCREEN_PATCH_SUCCESSFULLY_REVERTED".GetLocalizedString(),
                    "WIDESCREEN_PATCH_REVERTED_TITLE".GetLocalizedString(), MsgBoxButton.Ok, MsgBoxImage.Information);
            }
            else
            {
                await _patcherService.ViewContext.PopupService.ShowLocalized(patchReversalResult.Message,
                    "WIDESCREEN_PATCH_REVERSAL_ERROR_TITLE".GetLocalizedString(), MsgBoxButton.Ok, MsgBoxImage.Error);
            }
        }
    }
}
