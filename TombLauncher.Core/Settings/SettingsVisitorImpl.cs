using TombLauncher.Contracts.Localization;
using TombLauncher.Contracts.Settings;
using TombLauncher.Data.Database.UnitOfWork;

namespace TombLauncher.Core.Settings;

public class SettingsVisitorImpl : ISettingsVisitor
{
    private SettingsUnitOfWork _settingsUnitOfWork;
    public SettingsVisitorImpl(SettingsUnitOfWork settingsUnitOfWork)
    {
        _settingsUnitOfWork = settingsUnitOfWork;
    }
    public void Visit(ILocalizationManager localizationManager)
    {
        localizationManager.ChangeLanguage(_settingsUnitOfWork.GetApplicationLanguage());
    }
    
}