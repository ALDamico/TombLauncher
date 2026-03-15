using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using TombLauncher.Factories;

namespace TombLauncher.Extensions;

public static class MappingServiceCollectionExtensions
{
    public static IServiceCollection AddTombLauncherMappings(this IServiceCollection services)
    {
        services.AddSingleton(sp => MapperConfigurationFactory.GetMapperConfiguration(t => sp.GetService(t)!));
        services.AddSingleton<IMapper>(sp =>
        {
            var config = sp.GetRequiredService<MapperConfiguration>();
            return config.CreateMapper(t => sp.GetService(t)!);
        });
        return services;
    }
}
