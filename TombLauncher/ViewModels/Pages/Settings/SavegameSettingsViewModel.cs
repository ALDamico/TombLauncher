using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using TombLauncher.Data.Database.UnitOfWork;
using TombLauncher.Localization.Extensions;
using TombLauncher.Services;

namespace TombLauncher.ViewModels.Pages.Settings;

public partial class SavegameSettingsViewModel : SettingsSectionViewModelBase
{
    public SavegameSettingsViewModel(PageViewModel settingsPage) : base("SAVEGAMES", settingsPage)
    {
        _settingsService = Ioc.Default.GetRequiredService<SettingsService>();
        SyncSavegamesInfoCmd = new AsyncRelayCommand(SyncSavegamesInfo);
    }

    [ObservableProperty] private bool? _savegameBackupEnabled;
    [ObservableProperty] private bool _limitNumberOfVersions;
    [ObservableProperty] private int? _numberOfVersionsToKeep;
    [ObservableProperty] private int _savegameProcessingDelay;
    
    public ICommand SyncSavegamesInfoCmd { get; }
    private SettingsService _settingsService;

    private async Task SyncSavegamesInfo()
    {
        await _settingsService.SyncSavegames(SettingsPage);
    }
}