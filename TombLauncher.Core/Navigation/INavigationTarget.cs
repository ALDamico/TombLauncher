using System.Collections.ObjectModel;
using System.Windows.Input;

namespace TombLauncher.Core.Navigation;

public interface INavigationTarget
{
    void SetBusy(string busyMessage);
    void ClearBusy();
    void SetBusy(bool isBusy, string busyMessage);
    ObservableCollection<ITopBarCommand> TopBarCommands { get; set; }
    bool IsBusy { get; }
    string BusyMessage { get; }
    string CurrentFileName { get; }
    double? PercentageComplete { get; }
    ICommand CancelCmd { get; }
    bool IsCancelable { get; }
}