using Microsoft.Extensions.DependencyInjection;
using TombLauncher.Services;

namespace TombLauncher.Extensions;

public static class FileOperationsServiceCollectionExtensions
{
    public static IServiceCollection AddFileOperations(this IServiceCollection serviceCollection)
    {
        return serviceCollection.AddSingleton<IAppFileOperationsService, AppFileOperationsService>();
    }
}