using System;
using TombLauncher.Contracts.Localization;

namespace TombLauncher.Helpers;

internal static class LocalizationHelper
{
    public static Func<string, object[], string> GetStringGenerator(ILocalizationManager? localizationManager)
    {
        if (localizationManager != null)
        {
            return localizationManager.GetLocalizedString;
        }

        return string.Format;
    }
}
