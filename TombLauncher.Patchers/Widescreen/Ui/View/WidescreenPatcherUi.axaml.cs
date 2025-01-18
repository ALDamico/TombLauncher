using System.Windows.Input;
using Avalonia.Controls;
using JamSoft.AvaloniaUI.Dialogs.ViewModels;
using TombLauncher.Contracts.Patchers;
using TombLauncher.Patchers.Widescreen.Ui.ViewModel;

namespace TombLauncher.Patchers.Widescreen.Ui.View;

public partial class WidescreenPatcherUi : UserControl, IPatcherUi
{
    public WidescreenPatcherUi()
    {
        InitializeComponent();
        DataContext = new WidescreenPatcherUiViewModel();
    }
    
    private WidescreenPatcherUiViewModel _dataContext => DataContext as WidescreenPatcherUiViewModel;

    public string Title { get; set; }
    public IPatchParameters GetParameters()
    {
        return new WidescreenPatcherParameters()
        {
            HorizontalResolution = _dataContext.WidthResolution,
            TargetFolder = null,
            UpdateFov = _dataContext.ApplyFovCorrection,
            OriginalAspectRatio = default,
            OriginalCameraDistance = default,
            TargetAspectRatio = default,
            TargetCameraDistance = default,
            UpdateAspectRatio = default,
            UpdateCameraDistance = default
        };
    }

    public DialogViewModel ViewModel => _dataContext;
    public Control Control => this;

    public ICommand ApplyPatch(IPatcher patcher)
    {
        throw new NotImplementedException();
    }
}