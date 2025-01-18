using System.ComponentModel;
using System.Runtime.CompilerServices;
using JamSoft.AvaloniaUI.Dialogs.ViewModels;

namespace TombLauncher.Patchers.Widescreen.Ui.ViewModel;

public class WidescreenPatcherUiViewModel : DialogViewModel
{
    private Version _version;

    public Version Version
    {
        get => _version;
        set
        {
            _version = value;
            OnPropertyChanged();
        }
    }

    private string _description;

    public string Description
    {
        get => _description;
        set
        {
            _description = value;
            OnPropertyChanged();
        }
    }

    private bool _applyAspectRatioCorrection;

    public bool ApplyAspectRatioCorrection
    {
        get => _applyAspectRatioCorrection;
        set
        {
            _applyAspectRatioCorrection = value;
            OnPropertyChanged();
        }
    }

    private string _aspectRatio;

    public string AspectRatio
    {
        get => _aspectRatio;
        set
        {
            _aspectRatio = value;
            OnPropertyChanged();
        }
    }

    private bool _applyCameraDistanceCorrection;

    public bool ApplyCameraDistanceCorrection
    {
        get => _applyCameraDistanceCorrection;
        set
        {
            _applyCameraDistanceCorrection = value;
            OnPropertyChanged();
        }
    }

    private short _cameraDistance;

    public short CameraDistance
    {
        get => _cameraDistance;
        set
        {
            _cameraDistance = value;
            OnPropertyChanged();
        }
    }

    private bool _applyFovCorrection;

    public bool ApplyFovCorrection
    {
        get => _applyFovCorrection;
        set
        {
            _applyFovCorrection = value;
            OnPropertyChanged();
        }
    }

    private short _widthResolution;

    public short WidthResolution
    {
        get => _widthResolution;
        set
        {
            _widthResolution = value;
            OnPropertyChanged();
        }
    }
}