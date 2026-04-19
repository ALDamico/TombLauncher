using System.Windows.Input;

namespace TombLauncher.Core.Navigation;

public interface ITopBarCommand
{
    ICommand Command { get; set; }
    string Tooltip { get; set; }
    string Text { get; set; }
    Enum? Icon { get; set; }
}