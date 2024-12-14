using System.Globalization;
using TombLauncher.Contracts.Settings;
using TombLauncher.Data.Database.Repositories;
using TombLauncher.Data.Models;
using TombLauncher.Data.Shared;

namespace TombLauncher.Data.Database.UnitOfWork;

public class SettingsUnitOfWork: UnitOfWorkBase
{
    public SettingsUnitOfWork()
    {
        _settings = GetRepository<ApplicationSetting>();
    }
    private readonly Lazy<EfRepository<ApplicationSetting>> _settings;
    internal EfRepository<ApplicationSetting> Settings => _settings.Value;

    public CultureInfo GetApplicationLanguage()
    {
        var key = SettingsKeys.ApplicationLanguage;
        var setting = Settings.Get(s => s.SettingName == key).FirstOrDefault();
        if (setting == null)
        {
            return CultureInfo.CurrentUICulture;
        }

        return CultureInfo.GetCultureInfo(setting.StringValue);
    }

    public void UpdateApplicationLanguage(CultureInfo cultureInfo)
    {
        var languageName = cultureInfo.IetfLanguageTag;
        var setting = Settings.Get(s => s.SettingName == SettingsKeys.ApplicationLanguage).FirstOrDefault();
        if (setting == null)
        {
            setting = new ApplicationSetting()
            {
                SettingName = SettingsKeys.ApplicationLanguage,
                StringValue = cultureInfo.IetfLanguageTag
            };
            Settings.Insert(setting);
            Settings.Commit();
            return;
        }

        setting.StringValue = languageName;
        Settings.Update(setting);
        Settings.Commit();
    }
}