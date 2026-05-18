using Microsoft.Extensions.DependencyInjection;
using TombLauncher.Gamepad.SupportMatrix;

namespace TombLauncher.Gamepad.Extensions;

public static class GamepadServiceCollectionExtensions
{
    public static IServiceCollection AddGamepadSupport(this IServiceCollection services)
    {
        return services.AddSingleton<GamepadSupportMatrix>();
    }
}