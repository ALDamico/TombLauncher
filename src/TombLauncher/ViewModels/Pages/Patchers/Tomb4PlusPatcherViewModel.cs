using System;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using IconPacks.Avalonia.RemixIcon;
using TombLauncher.Contracts.Downloaders;
using TombLauncher.Core.Patchers;
using TombLauncher.Localization.Extensions;
using TombLauncher.Patchers.Shared;
using TombLauncher.Patchers.Tomb4Plus.Enums;
using TombLauncher.Services.Patchers.Tomb4Plus;

namespace TombLauncher.ViewModels.Pages.Patchers;

public partial class Tomb4PlusPatcherViewModel : ObservableObject, IPatcherParametersViewModel
{
    private readonly Tomb4PlusPatcherService _tomb4PlusPatcherService;
    private IGameMetadataLite _gameMetadata;

    public Tomb4PlusPatcherViewModel(Tomb4PlusPatcherService tomb4PlusPatcherService)
    {
        _tomb4PlusPatcherService = tomb4PlusPatcherService;
    }
    [ObservableProperty]
    public partial bool CanApplyPatch { get; set; }
    [ObservableProperty]
    public partial bool CanRevertPatch { get; set; }
    public ProgressLogger ProgressLogger { get; set; }
    public async Task ApplyPatch()
    {
        await _tomb4PlusPatcherService.ApplyPatch(_gameMetadata, ProgressLogger, SyntaxFile.Early,
            CancellationToken.None); // TODO Allow Syntax File selection
    }

    public Task RevertPatch()
    {
        throw new NotImplementedException();
    }

    public Task Init(IGameMetadataLite? gameMetadata, ProgressLogger progressLogger)
    {
        _gameMetadata = gameMetadata!;
        ProgressLogger = progressLogger;
        return Task.CompletedTask;
    }

    public string ApplyPatchButtonCaption => "CONVERT_TO_TOMB4PLUS".GetLocalizedString();
    public string RevertPatchButtonCaption => "REVERT_TO_ORIGINAL_EXECUTABLE".GetLocalizedString();
    public Enum? ApplyPatchButtonIcon => PackIconRemixIconKind.PlayLine;
    public Enum? RevertPatchButtonIcon => PackIconRemixIconKind.RewindLine;
}