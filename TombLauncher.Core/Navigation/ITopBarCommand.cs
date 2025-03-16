using System.Windows.Input;
using Material.Icons;

namespace TombLauncher.Core.Navigation;

public interface ITopBarCommand
{
    ICommand Command { get; set; }
    string Tooltip { get; set; }
    string Text { get; set; }
    MaterialIconKind Icon { get; set; }
}