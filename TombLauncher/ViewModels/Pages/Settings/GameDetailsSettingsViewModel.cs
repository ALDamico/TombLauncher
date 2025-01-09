using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace TombLauncher.ViewModels.Pages.Settings;

public partial class GameDetailsSettingsViewModel : SettingsSectionViewModelBase
{
    public GameDetailsSettingsViewModel() : base("GAME DETAILS")
    {
    }

    [ObservableProperty] private bool _askForConfirmationBeforeWalkthrough;
    [ObservableProperty] private bool _useInternalViewerIfAvailable;
}