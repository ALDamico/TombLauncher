using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.Input;

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
    IRelayCommand CancelCommand { get; }
    bool IsCancelable { get; }
}