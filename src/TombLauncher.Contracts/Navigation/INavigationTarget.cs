using System.Collections.ObjectModel;
using System.Windows.Input;

namespace TombLauncher.Contracts.Navigation;

public interface INavigationTarget
{
    /// <summary>
    /// Sets the busy state with a message. Use when the busy state must span method boundaries
    /// (e.g. fire-and-forget process launch cleared by a callback). Prefer <see cref="BusyScope"/> otherwise.
    /// </summary>
    void SetBusy(string busyMessage);
    IDisposable BusyScope(string busyMessage);
    void ClearBusy();
    ObservableCollection<ITopBarCommand> TopBarCommands { get; set; }
    bool IsBusy { get; }
    string BusyMessage { get; }
    string CurrentFileName { get; }
    double? PercentageComplete { get; }
    bool IsCancelable { get; }
    public ICommand CancelCmd { get; }
}