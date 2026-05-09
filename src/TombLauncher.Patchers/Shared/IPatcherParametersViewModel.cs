using System.ComponentModel;
using TombLauncher.Contracts.Downloaders;
using TombLauncher.Core.Patchers;

namespace TombLauncher.Patchers.Shared;

public interface IPatcherParametersViewModel : INotifyPropertyChanged
{
    public bool CanApplyPatch { get; }
    public bool CanRevertPatch { get; }
    
    public ProgressLogger ProgressLogger { get; set; }

    public Task ApplyPatch();

    public Task RevertPatch();

    public Task Init(IGameMetadataLite? gameMetadata, ProgressLogger progressLogger);
}