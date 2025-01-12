using System.Windows.Input;

namespace TombLauncher.Contracts.Patchers;

public interface IPatcherUi
{
    string Title { get; set; }
    ICommand ApplyPatch(IPatcher patcher);
}