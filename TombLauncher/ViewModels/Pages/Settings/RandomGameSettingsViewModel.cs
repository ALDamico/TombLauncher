using System.ComponentModel.DataAnnotations;
using CommunityToolkit.Mvvm.ComponentModel;

namespace TombLauncher.ViewModels.Pages.Settings;

public partial class RandomGameSettingsViewModel : SettingsSectionViewModelBase
{
    public RandomGameSettingsViewModel() : base("RANDOM GAME")
    {
    }

    [Range(1, 15, ErrorMessage = "Allowed values: 1-15")]
    
    public int MaxRerolls
    {
        get => _maxRerolls;
        set => SetProperty(ref _maxRerolls, value, true);
    }
    private int _maxRerolls;
}