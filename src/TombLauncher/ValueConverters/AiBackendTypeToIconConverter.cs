using System;
using System.Globalization;
using Avalonia.Data.Converters;
using TombLauncher.Contracts.Enums;

namespace TombLauncher.ValueConverters;

public class AiBackendTypeToIconConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is AiBackendType backendType)
        {
            return backendType switch
            {
                AiBackendType.Ollama => "avares://TombLauncher/Assets/Ai/ollama.svg",
                AiBackendType.LmStudio => "avares://TombLauncher/Assets/Ai/lmstudio.svg",
                _ => null
            };
        }

        return null;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}