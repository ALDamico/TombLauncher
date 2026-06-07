using System.Collections.ObjectModel;
using System.Linq;
using TombLauncher.Contracts.Enums;
using TombLauncher.Core.Extensions;
using TombLauncher.Gamepad;
using TombLauncher.Gamepad.SupportMatrix;

namespace TombLauncher.ViewModels.Pages;

public class GamepadSupportMatrixViewModel : PageViewModel
{
    public GamepadSupportMatrixViewModel(GamepadSupportMatrix gamepadSupportMatrix)
    {
        Entries = gamepadSupportMatrix.GetEntries()
            .Where(e => e.Engine != GameEngine.Unknown)
            .ToObservableCollection();
    }

    public ObservableCollection<GamepadSupportMatrixEntry> Entries { get; }
}