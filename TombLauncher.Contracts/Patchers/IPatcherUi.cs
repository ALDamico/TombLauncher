using System.ComponentModel;
using System.Windows.Input;
using Avalonia.Controls;
using JamSoft.AvaloniaUI.Dialogs.ViewModels;

namespace TombLauncher.Contracts.Patchers;

public interface IPatcherUi
{
    string Title { get; set; }

    IPatchParameters GetParameters();
    DialogViewModel ViewModel { get; }
    Control Control { get; }
}