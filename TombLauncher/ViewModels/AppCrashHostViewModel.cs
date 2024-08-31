using CommunityToolkit.Mvvm.ComponentModel;
using TombLauncher.Dto;

namespace TombLauncher.ViewModels;

public partial class AppCrashHostViewModel : DialogViewModelBase
{
    public AppCrashHostViewModel()
    {
        AcceptCommandText = "Accetta";
        CancelCommandText = "Annulla";
    }
    [ObservableProperty] private AppCrashDto _crash;
    protected override void Accept()
    {
        base.Accept();
    }

    protected override bool CanAcceptInner()
    {
        return true;
    }

    protected override void Cancel()
    {
        
    }
}