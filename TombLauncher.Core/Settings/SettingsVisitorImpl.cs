using System.Reflection;
using CommunityToolkit.Mvvm.DependencyInjection;
using TombLauncher.Contracts.Downloaders;
using TombLauncher.Contracts.Localization;
using TombLauncher.Contracts.Settings;
using TombLauncher.Contracts.Utils;
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

    public void Visit(IGameDownloadManager downloadManager)
    {
        downloadManager.Downloaders.Clear();
        var enabledDownloaders = _settingsUnitOfWork.GetDownloaderConfigurations(true);
        foreach (var downloader in enabledDownloaders)
        {
            var className = downloader.ClassName;
            var downloaderType = ReflectionUtils.GetTypeByName(className);
            if (downloaderType != null)
            {
                var downloaderImpl = (IGameDownloader)Ioc.Default.GetService(downloaderType);
                downloadManager.Downloaders.Add(downloaderImpl);
            }
        }
    }
}