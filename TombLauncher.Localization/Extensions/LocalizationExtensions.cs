using CommunityToolkit.Mvvm.DependencyInjection;
using TombLauncher.Contracts.Localization;

namespace TombLauncher.Localization.Extensions;

public static class LocalizationExtensions
{
    public static string GetLocalizedString(this string s)
    {
        return s.GetLocalizedString(null);
    }

    public static string GetLocalizedString(this string s, params object[] args)
    {
        var localizationManager = Ioc.Default.GetRequiredService<ILocalizationManager>();
        return localizationManager.GetLocalizedString(s, args);
    }
}