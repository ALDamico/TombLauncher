using System.Windows.Input;

namespace TombLauncher.Contracts.Navigation;

public interface ITopBarCommand
{
    ICommand Command { get; set; }
    string Tooltip { get; set; }
    string Text { get; set; }
    Enum Icon { get; set; }
}