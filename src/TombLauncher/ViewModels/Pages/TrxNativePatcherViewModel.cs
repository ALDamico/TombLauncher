using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using JamSoft.AvaloniaUI.Dialogs.MsgBox;
using TombLauncher.Contracts.Downloaders;
using TombLauncher.Core.Patchers;
using TombLauncher.Extensions;
using TombLauncher.Patchers.Shared;
using TombLauncher.Services.Patchers.TrxNative;

namespace TombLauncher.ViewModels.Pages;

public partial class TrxNativePatcherViewModel : ObservableObject, IPatcherParametersViewModel
{
    public TrxNativePatcherViewModel(TrxNativePatcherService trxNativePatcherService)
    {
        _trxNativePatcherService = trxNativePatcherService;
    }

    [ObservableProperty] private string _detectedVersion = "";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanRevertPatch))]
    private bool _canApplyPatch;

    public bool CanRevertPatch => !CanApplyPatch;
    public ProgressLogger ProgressLogger { get; set; } = null!;

    private IGameMetadataLite? _gameMetadata;
    private readonly TrxNativePatcherService _trxNativePatcherService;

    public async Task ApplyPatch()
    {
        ProgressLogger.Info("APPLYING_TRX_NATIVE_PATCH");
        var patchResult = await _trxNativePatcherService.ApplyPatch(_gameMetadata!, ProgressLogger, CancellationToken.None);
        if (patchResult.IsSuccessful)
        {
            CanApplyPatch = false;
            await _trxNativePatcherService.ViewContext.PopupService.ShowLocalized("NATIVE_PATCH_SUCCESSFULLY_APPLIED",
                "NATIVE_PATCH_APPLIED_TITLE", MsgBoxButton.Ok, MsgBoxImage.Information);
        }
        else
        {
            await _trxNativePatcherService.ViewContext.PopupService.ShowLocalized(patchResult.Message,
                "NATIVE_PATCH_ERROR_TITLE", MsgBoxButton.Ok, MsgBoxImage.Error);
        }
    }

    public async Task RevertPatch()
    {
        ProgressLogger.Info("RESTORING_TRX_ORIGINAL_EXECUTABLE");

        var patchResult =
            await _trxNativePatcherService.RevertPatch(_gameMetadata!, ProgressLogger, CancellationToken.None);
        if (patchResult.IsSuccessful)
        {
            CanApplyPatch = true;
            await _trxNativePatcherService.ViewContext.PopupService.ShowLocalized("NATIVE_PATCH_SUCCESSFULLY_REVERTED",
                "NATIVE_PATCH_REVERTED_TITLE", MsgBoxButton.Ok, MsgBoxImage.Information);
        }
        else
        {
            await _trxNativePatcherService.ViewContext.PopupService.ShowLocalized(patchResult.Message,
                "NATIVE_PATCH_REVERSAL_ERROR_TITLE", MsgBoxButton.Ok, MsgBoxImage.Error);
        }
    }

    public async Task Init(IGameMetadataLite? gameMetadata, ProgressLogger progressLogger)
    {
        _gameMetadata = gameMetadata;
        ProgressLogger = progressLogger;

        var executablePath = Path.Combine(_gameMetadata!.InstallDirectory!, _gameMetadata.ExecutablePath!);
        var versionInfo = _trxNativePatcherService.GetVersionInfo(executablePath, new Progress<string>(msg => ProgressLogger.Info(msg)));
        DetectedVersion = $"{versionInfo.ExecutableName} {versionInfo.Version}";

        CanApplyPatch =
            !await _trxNativePatcherService.IsAlreadyApplied(_gameMetadata.Id, ProgressLogger, CancellationToken.None);
    }
}