using System.Reflection;
using JamSoft.AvaloniaUI.Dialogs;
using Microsoft.Extensions.DependencyInjection;
using TombLauncher.Services;

namespace TombLauncher.Extensions;

public static class ModalsServiceCollectionExtensions
{
    public static IServiceCollection AddModals(this IServiceCollection serviceCollection)
    {
        return serviceCollection.AddSingleton<IPopupService>(_ => new PopupService(
            DialogServiceFactory.CreateMessageBoxService(),
            DialogServiceFactory.Create(new DialogServiceConfiguration()
            {
                ApplicationName = "Tomb Launcher",
                UseApplicationNameInTitle = true,
                ViewsAssemblyName = Assembly.GetExecutingAssembly().GetName().Name
            })));
    }
}