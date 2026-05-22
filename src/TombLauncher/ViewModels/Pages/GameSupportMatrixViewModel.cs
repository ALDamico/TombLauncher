using System.Collections.ObjectModel;
using System.Linq;
using TombLauncher.Contracts.Enums;
using TombLauncher.Contracts.PlatformSpecific;
using TombLauncher.Contracts.PlatformSpecific.Models;
using TombLauncher.Core.Extensions;

namespace TombLauncher.ViewModels.Pages;

public class GameSupportMatrixViewModel : PageViewModel
{
    public GameSupportMatrixViewModel(IPlatformSpecificFeatures platformSpecificFeatures)
    {
        SupportMatrixEntries = platformSpecificFeatures.SupportMatrix
            .GetEntries()
            .Where(e => e.Engine != GameEngine.Unknown)
            .ToObservableCollection();
    }
    
    public ObservableCollection<GameSupportMatrixEntry> SupportMatrixEntries { get; }
}