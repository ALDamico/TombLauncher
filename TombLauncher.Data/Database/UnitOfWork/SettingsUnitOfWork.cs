using System.Globalization;
using System.Reflection;
using TombLauncher.Contracts.Downloaders;
using TombLauncher.Contracts.Dtos;
using TombLauncher.Contracts.Utils;
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

    public void UpdateDownloaderConfigurations(IEnumerable<DownloaderConfigDto> dtos)
    {
        foreach (var dto in dtos)
        {
            var prioritySettingsKey = GetDownloaderSettingKey(dto.ClassName, SettingsKeys.Priority);
            var targetEntity = Settings.Get(s => s.SettingName == prioritySettingsKey).SingleOrDefault();
            if (targetEntity == null)
            {
                InsertSetting(prioritySettingsKey, dto.Priority);
            }
            else
            {
                targetEntity.IntValue = dto.Priority;
                Settings.Update(targetEntity);
            }

            var isEnabledSettingsKey = GetDownloaderSettingKey(dto.ClassName, SettingsKeys.IsEnabled);
            var targetEntityIsEnabled = Settings.Get(s => s.SettingName == isEnabledSettingsKey).SingleOrDefault();
            if (targetEntityIsEnabled == null)
            {
                InsertSetting(isEnabledSettingsKey, dto.IsEnabled);
            }
            else
            {
                targetEntityIsEnabled.BoolValue = dto.IsEnabled;
                Settings.Update(targetEntity);
            }
        }
        
        Settings.Commit();
    }

    private void InsertSetting(string settingName, int? targetValue)
    {
        var entity = new ApplicationSetting()
        {
            SettingName = settingName,
            IntValue = targetValue
        };
        Settings.Insert(entity);
    }

    private void InsertSetting(string settingName, bool? targetValue)
    {
        var entity = new ApplicationSetting()
        {
            SettingName = settingName,
            BoolValue = targetValue
        };
        Settings.Insert(entity);
    }

    private void InsertSetting(string settingName, string targetValue)
    {
        var entity = new ApplicationSetting()
        {
            SettingName = settingName,
            StringValue = targetValue
        };
        Settings.Insert(entity);
    }

    private void InsertSetting(string settingName, double? targetValue)
    {
        var entity = new ApplicationSetting()
        {
            SettingName = settingName,
            DoubleValue = targetValue
        };
        Settings.Insert(entity);
    }

    private void InsertSetting(string settingName, DateTime? targetValue)
    {
        var entity = new ApplicationSetting()
        {
            SettingName = settingName,
            DateTimeValue = targetValue
        };
        Settings.Insert(entity);
    }

    public List<DownloaderConfigDto> GetDownloaderConfigurations(bool enabledOnly = false)
    {
        var dtos = new List<DownloaderConfigDto>();
        var downloaders = ReflectionUtils.GetImplementors<IGameDownloader>(BindingFlags.NonPublic);
        var priority = 1;
        foreach (var downloader in downloaders)
        {
            var dto = new DownloaderConfigDto()
            {
                BaseUrl = downloader.BaseUrl,
                ClassName = downloader.GetType().Name,
                DisplayName = downloader.DisplayName,
                IsEnabled = true,
                Priority = priority++
            };
            dtos.Add(dto);
        }

        var downloaderSettings = GetDownloaderSettings(dtos.Select(dto => dto.ClassName));
        foreach (var setting in downloaderSettings)
        {
            var split = setting.SettingName.Split("-").Take(3).ToArray();
            var className = split[0];
            var settingName = split[2];
            var targetDto = dtos.SingleOrDefault(dto => dto.ClassName == className);
            if (targetDto == null)
                continue;
            switch (settingName)
            {
                case SettingsKeys.IsEnabled:
                    targetDto.IsEnabled = setting.BoolValue.GetValueOrDefault();
                    break;
                case SettingsKeys.Priority:
                    targetDto.Priority = setting.IntValue.GetValueOrDefault();
                    break;
            }
        }

        if (enabledOnly)
        {
            return dtos.Where(dto => dto.IsEnabled).OrderBy(dto => dto.Priority).ToList();
        }

        return dtos.OrderBy(dto => dto.Priority).ToList();
    }

    private string GetDownloaderSettingKey(string className, string settingName)
    {
        return string.Join("-", className, SettingsKeys.Downloader, settingName);
    }

    private List<ApplicationSetting> GetDownloaderSettings(IEnumerable<string> classNames)
    {
        var settingsNames = new List<string>();
        foreach (var className in classNames)
        {
            var isEnabledSetting = GetDownloaderSettingKey(className, SettingsKeys.IsEnabled);
            var prioritySetting = GetDownloaderSettingKey(className, SettingsKeys.Priority);
            settingsNames.Add(isEnabledSetting);
            settingsNames.Add(prioritySetting);
        }

        return Settings.GetAll().Join(settingsNames, setting => setting.SettingName, s => s, (setting, s) => setting).ToList();
    }
}