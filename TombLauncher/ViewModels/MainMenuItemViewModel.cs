using Material.Icons;

namespace TombLauncher.ViewModels;

public class MainMenuItemViewModel : ViewModelBase
{
    private MaterialIconKind _icon;
    public MaterialIconKind Icon
    {
        get => _icon;
        set => SetProperty(ref _icon, value);
    }

    private string _text;

    public string Text
    {
        get => _text;
        set => SetProperty(ref _text, value);
    }

    private string _tooltip;

    public string ToolTip
    {
        get => _tooltip;
        set => SetProperty(ref _tooltip, value);
    }
}