using TombLauncher.Contracts.Downloaders;
using TombLauncher.Contracts.Localization;

namespace TombLauncher.Contracts.Settings;

public interface ISettingsVisitor
{
    void Visit(ILocalizationManager localizationManager);
    void Visit(IGameDownloadManager downloadManager);
}