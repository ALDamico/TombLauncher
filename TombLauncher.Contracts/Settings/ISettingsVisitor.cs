using TombLauncher.Contracts.Localization;

namespace TombLauncher.Contracts.Settings;

public interface ISettingsVisitor
{
    void Visit(ILocalizationManager localizationManager);
}