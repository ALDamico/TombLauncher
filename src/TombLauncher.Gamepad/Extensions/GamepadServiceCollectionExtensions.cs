using Microsoft.Extensions.DependencyInjection;
using TombLauncher.Gamepad.Configuration;
using TombLauncher.Gamepad.Services;
using TombLauncher.Gamepad.SupportMatrix;

namespace TombLauncher.Gamepad.Extensions;

public static class GamepadServiceCollectionExtensions
{
    public static IServiceCollection AddGamepadSupport(this IServiceCollection services)
    {
        return services.AddSingleton<GamepadSupportMatrix>()
            .AddSingleton<NullGamepadService>()
            .AddKeyedSingleton<IGamepadService, AntiMicroXGamepadService>(GamepadTool.AntiMicroX)
            .AddTransient<IGamepadService>(sp =>
            {
                var config = sp.GetRequiredService<IGamepadConfig>();
                if (config.GamepadTool is null or GamepadTool.None)
                    return sp.GetRequiredService<NullGamepadService>();

                return sp.GetRequiredKeyedService<IGamepadService>(config.GamepadTool);
            });
    }
}