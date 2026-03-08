using System.Linq;

namespace TombLauncher.Helpers;

internal static class ByteSizeFormatHelper
{
    public static string Format(double value, string[] unitLabels)
    {
        var tmpVal = value;
        var unit = string.Empty;

        foreach (var multiple in unitLabels.Select((m, i) => (m, i)))
        {
            if (multiple.i > 0)
            {
                tmpVal /= 1024;
            }

            unit = multiple.m;

            if (tmpVal <= 1024)
            {
                break;
            }
        }

        return $"{tmpVal:F2} {unit}";
    }
}
