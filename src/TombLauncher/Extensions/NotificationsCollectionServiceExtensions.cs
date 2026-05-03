using Microsoft.Extensions.DependencyInjection;
using TombLauncher.Services;

namespace TombLauncher.Extensions;

public static class NotificationsCollectionServiceExtensions
{
    public static IServiceCollection AddNotifications(this IServiceCollection serviceCollection)
    {
        return serviceCollection.AddSingleton<NotificationService>();
    }
}