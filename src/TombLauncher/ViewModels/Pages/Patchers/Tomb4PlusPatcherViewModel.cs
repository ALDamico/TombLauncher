using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using IconPacks.Avalonia.RemixIcon;
using TombLauncher.Contracts.Downloaders;
using TombLauncher.Core.Extensions;
using TombLauncher.Core.Patchers;
using TombLauncher.Localization.Extensions;
using TombLauncher.Patchers.Shared;
using TombLauncher.Patchers.Tomb4Plus.Enums;
using TombLauncher.Services.Patchers.Tomb4Plus;
using TombLauncher.Utils;

namespace TombLauncher.ViewModels.Pages.Patchers;

public partial class Tomb4PlusPatcherViewModel : ObservableObject, IPatcherParametersViewModel
{
    private readonly Tomb4PlusPatcherService _tomb4PlusPatcherService;
    private IGameMetadataLite _gameMetadata = null!;

    public Tomb4PlusPatcherViewModel(Tomb4PlusPatcherService tomb4PlusPatcherService)
    {
        _tomb4PlusPatcherService = tomb4PlusPatcherService;
        AvailableSyntaxFiles = EnumUtils.GetEnumViewModels<SyntaxFile>().ToObservableCollection();
        SelectedSyntaxFile = AvailableSyntaxFiles.FirstOrDefault();
        CanApplyPatch = true;
        CanRevertPatch = false;
    }
    [ObservableProperty]
    public partial bool CanApplyPatch { get; set; }
    [ObservableProperty]
    public partial bool CanRevertPatch { get; set; }
    public ObservableCollection<EnumViewModel<SyntaxFile>> AvailableSyntaxFiles { get; }
    [ObservableProperty]
    public partial EnumViewModel<SyntaxFile>? SelectedSyntaxFile { get; set; }

    public ProgressLogger ProgressLogger { get; set; } = null!;
    public async Task ApplyPatch()
    {
        CanApplyPatch = false;
        await _tomb4PlusPatcherService.ApplyPatch(_gameMetadata, ProgressLogger, SelectedSyntaxFile!.Value,
            CancellationToken.None);
        CanRevertPatch = true;
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
    public Enum ApplyPatchButtonIcon => PackIconRemixIconKind.PlayLine;
    public Enum RevertPatchButtonIcon => PackIconRemixIconKind.RewindLine;
}