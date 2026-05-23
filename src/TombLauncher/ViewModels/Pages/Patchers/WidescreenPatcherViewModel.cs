using System;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using IconPacks.Avalonia.RemixIcon;
using JamSoft.AvaloniaUI.Dialogs.MsgBox;
using TombLauncher.Contracts.Downloaders;
using TombLauncher.Contracts.Enums;
using TombLauncher.Core.Patchers;
using TombLauncher.Extensions;
using TombLauncher.Localization.Extensions;
using TombLauncher.Patchers.Shared;
using TombLauncher.Patchers.Widescreen;
using TombLauncher.Services.Patchers.Widescreen;

namespace TombLauncher.ViewModels.Pages.Patchers;

public partial class WidescreenPatcherViewModel : ObservableObject, IPatcherParametersViewModel
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanApplyPatchByFormState))]
    [NotifyPropertyChangedFor(nameof(CanApplyPatch))]
    public partial bool UpdateAspectRatio { get; set; }

    [ObservableProperty]
    public partial float AspectRatioWidth { get; set; } = 16;

    [ObservableProperty]
    public partial float AspectRatioHeight { get; set; } = 9;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanApplyPatchByFormState))]
    [NotifyPropertyChangedFor(nameof(CanApplyPatch))]
    public partial bool UpdateCameraDistance { get; set; }

    [ObservableProperty]
    public partial CameraDistanceOptionViewModel SelectedCameraDistanceOption { get; set; }

    [ObservableProperty]
    public partial short CustomCameraDistance { get; set; } = (short)CameraDistancePreset.OneAndAHalf;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanApplyPatchByFormState))]
    [NotifyPropertyChangedFor(nameof(CanApplyPatch))]
    public partial bool UpdateFov { get; set; }

    [ObservableProperty]
    public partial int TargetFov { get; set; } = 1920;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanApplyPatchByFormState))]
    [NotifyPropertyChangedFor(nameof(CanApplyPatch))]
    public partial bool Update60Fps { get; set; }

    [ObservableProperty]
    public partial bool Is60FpsAvailable { get; set; }

    [ObservableProperty]
    public partial string? Fps60TooltipText { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanRevertPatch))]
    [NotifyPropertyChangedFor(nameof(CanApplyPatch))]
    private bool _canApplyPatchByFileState;
    public bool CanRevertPatch => !CanApplyPatchByFileState;
    public ProgressLogger ProgressLogger { get; set; } = null!;

    public bool CanApplyPatch => CanApplyPatchByFileState && CanApplyPatchByFormState;
    
    public bool CanApplyPatchByFormState => Update60Fps || UpdateAspectRatio || UpdateFov || UpdateCameraDistance;

    public ObservableCollection<CameraDistanceOptionViewModel> CameraDistanceOptions { get; }

    private IGameMetadataLite? _gameMetadata;
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

    public async Task Init(IGameMetadataLite? parameter, ProgressLogger progressLogger)
    {
        _gameMetadata = parameter;
        ProgressLogger = progressLogger;
        ProgressLogger.Info("CHECKING_60_FPS_SUPPORT");
        Is60FpsAvailable = _patcherService.Check60FpsSupport(_gameMetadata?.InstallDirectory ?? "");
        if (!Is60FpsAvailable)
        {
            Fps60TooltipText = "FPS_60_NOT_SUPPORTED".GetLocalizedString(_gameMetadata?.GameEngine.GetDescription()!);
            ProgressLogger.Warn(Fps60TooltipText);
        }

        var isPatchApplied = await _patcherService.IsAlreadyApplied(_gameMetadata!.Id, ProgressLogger, CancellationToken.None);
        if (!isPatchApplied.IsSuccessful)
        {
            CanApplyPatchByFileState = true;
        }
    }

    public string ApplyPatchButtonCaption => "APPLY_WIDESCREEN_PATCH".GetLocalizedString();
    public string RevertPatchButtonCaption => "REVERT_WIDESCREEN_PATCH".GetLocalizedString();
    public Enum ApplyPatchButtonIcon => PackIconRemixIconKind.PlayLine;
    public Enum RevertPatchButtonIcon => PackIconRemixIconKind.RewindLine;

    private int GetCameraDistance()
    {
        if (SelectedCameraDistanceOption.IsCustom)
            return CustomCameraDistance;

        return (int)SelectedCameraDistanceOption.Value;
    }

    public async Task ApplyPatch()
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

        ProgressLogger.Info("APPLYING_WIDESCREEN_PATCH");

        var patchResult = await _patcherService.ApplyPatch(_gameMetadata, parameters, ProgressLogger, CancellationToken.None);

        if (patchResult.IsSuccessful)
        {
            CanApplyPatchByFileState = false;
            ProgressLogger.Success("WIDESCREEN_PATCH_SUCCESSFULLY_APPLIED");
            await _patcherService.ViewContext.PopupService.ShowLocalized(
                "WIDESCREEN_PATCH_SUCCESSFULLY_APPLIED",
                "WIDESCREEN_PATCH_APPLIED_TITLE", MsgBoxButton.Ok, MsgBoxImage.Information);
        }
        else
        {
            ProgressLogger.Error(patchResult.Message);
            await _patcherService.ViewContext.PopupService.ShowLocalized(patchResult.Message,
                "WIDESCREEN_PATCH_ERROR_TITLE", MsgBoxButton.Ok, MsgBoxImage.Error);
        }
    }

    public async Task RevertPatch()
    {
        ProgressLogger.Info("REVERTING_WIDESCREEN_PATCH");

        var patchReversalResult =
            await _patcherService.RevertPatch(_gameMetadata!, ProgressLogger, CancellationToken.None);
        if (patchReversalResult.IsSuccessful)
        {
            CanApplyPatchByFileState = true;
            ProgressLogger.Success("WIDESCREEN_PATCH_SUCCESSFULLY_REVERTED");
            await _patcherService.ViewContext.PopupService.ShowLocalized(
                "WIDESCREEN_PATCH_SUCCESSFULLY_REVERTED",
                "WIDESCREEN_PATCH_REVERTED_TITLE", MsgBoxButton.Ok, MsgBoxImage.Information);
        }
        else
        {
            ProgressLogger.Error(patchReversalResult.Message);
            await _patcherService.ViewContext.PopupService.ShowLocalized(patchReversalResult.Message,
                "WIDESCREEN_PATCH_REVERSAL_ERROR_TITLE", MsgBoxButton.Ok, MsgBoxImage.Error);
        }
    }
}
