using System.Windows.Input;
using IconPacks.Avalonia.RemixIcon;

namespace TombLauncher.Core.Navigation;

public interface ITopBarCommand
{
    ICommand Command { get; set; }
    string Tooltip { get; set; }
    string Text { get; set; }
    PackIconRemixIconKind Icon { get; set; }
}