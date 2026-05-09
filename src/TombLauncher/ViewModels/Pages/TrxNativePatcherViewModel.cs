using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using JamSoft.AvaloniaUI.Dialogs.MsgBox;
using TombLauncher.Extensions;
using TombLauncher.Localization.Extensions;
using TombLauncher.Services.Patchers.TrxNative;

namespace TombLauncher.ViewModels.Pages;

public partial class TrxNativePatcherViewModel : PageViewModel
{
    public TrxNativePatcherViewModel(TrxNativePatcherService trxNativePatcherService)
    {
        _trxNativePatcherService = trxNativePatcherService;
    }

    [ObservableProperty] private string _detectedVersion = "";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanRevertPatch))]
    [NotifyCanExecuteChangedFor(nameof(ApplyPatchCommand))]
    [NotifyCanExecuteChangedFor(nameof(RevertPatchCommand))]
    private bool _canApplyPatch;

    public bool CanRevertPatch => !CanApplyPatch;

    private GameMetadataViewModel? _gameMetadata;
    private readonly TrxNativePatcherService _trxNativePatcherService;

    [RelayCommand(CanExecute = nameof(CanApplyPatch))]
    private async Task ApplyPatch()
    {
        var progress = new Progress<string>(msg => BusyMessage = msg);
        using (BusyScope("APPLYING_TRX_NATIVE_PATCH".GetLocalizedString()))
        {
            var patchResult = await _trxNativePatcherService.ApplyPatch(_gameMetadata!, progress, CancellationToken.None);
            if (patchResult.IsSuccessful)
            {
                CanApplyPatch = false;
                await _trxNativePatcherService.ViewContext.PopupService.ShowLocalized("NATIVE_PATCH_SUCCESSFULLY_APPLIED",
                    "NATIVE_PATCH_APPLIED_TITLE", MsgBoxButton.Ok, MsgBoxImage.Information);
            }
            else
            {
                await _trxNativePatcherService.ViewContext.PopupService.ShowLocalized(patchResult.Message,
                    "NATIVE_PATCH_ERROR_TITLE".GetLocalizedString(), MsgBoxButton.Ok, MsgBoxImage.Error);
            }
        }
    }

    [RelayCommand(CanExecute = nameof(CanRevertPatch))]
    private async Task RevertPatch()
    {
        var progress = new Progress<string>(msg => BusyMessage = msg);
        using (BusyScope("RESTORING_TRX_ORIGINAL_EXECUTABLE".GetLocalizedString()))
        {
            var patchResult = await _trxNativePatcherService.RevertPatch(_gameMetadata!, progress, CancellationToken.None);
            if (patchResult.IsSuccessful)
            {
                CanApplyPatch = true;
                await _trxNativePatcherService.ViewContext.PopupService.ShowLocalized("NATIVE_PATCH_SUCCESSFULLY_REVERTED",
                    "NATIVE_PATCH_REVERTED_TITLE", MsgBoxButton.Ok, MsgBoxImage.Information);
            }
            else
            {
                await _trxNativePatcherService.ViewContext.PopupService.ShowLocalized(patchResult.Message,
                    "NATIVE_PATCH_REVERSAL_ERROR_TITLE".GetLocalizedString(), MsgBoxButton.Ok, MsgBoxImage.Error);
            }
        }
    }

    public override async Task OnNavigatedTo(object parameter)
    {
        var progress = new Progress<string>(msg => BusyMessage = msg);
        using (BusyScope("READING_GAME_INFO".GetLocalizedString()))
        {
            _gameMetadata = (GameMetadataViewModel)parameter;

            var executablePath = Path.Combine(_gameMetadata.InstallDirectory!, _gameMetadata.ExecutablePath!);
            var versionInfo = _trxNativePatcherService.GetVersionInfo(executablePath, progress);
            DetectedVersion = $"{versionInfo.ExecutableName} {versionInfo.Version}";

            CanApplyPatch =
                !await _trxNativePatcherService.IsAlreadyApplied(_gameMetadata.Id, progress, CancellationToken.None);
        }
        
        await base.OnNavigatedTo(parameter);
    }
}