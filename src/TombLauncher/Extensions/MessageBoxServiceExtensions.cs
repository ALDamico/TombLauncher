using System.Reflection;
using System.Threading.Tasks;
using JamSoft.AvaloniaUI.Dialogs;
using JamSoft.AvaloniaUI.Dialogs.MsgBox;
using Microsoft.Extensions.DependencyInjection;
using TombLauncher.Core.Extensions;
using TombLauncher.Localization.Extensions;
using TombLauncher.Services;

namespace TombLauncher.Extensions;

public static class MessageBoxServiceExtensions
{
    public static Task<MsgBoxResult> ShowLocalized(this IMessageBoxService service, string? caption, string? messageBoxText, MsgBoxButton button,
        MsgBoxImage icon = MsgBoxImage.None, string? noButtonText = null, string? yesButtonText = null,
        string? cancelButtonText = null, string? checkBoxText = null)
    {
        noButtonText = GetActualButtonText(noButtonText, "No");
        yesButtonText = GetActualButtonText(yesButtonText, "Yes");
        cancelButtonText = GetActualButtonText(cancelButtonText, "Cancel");
        checkBoxText = checkBoxText?.GetLocalizedString();
        caption = caption?.GetLocalizedString();
        messageBoxText = messageBoxText?.GetLocalizedString();

        return service.Show(caption ?? string.Empty, messageBoxText ?? string.Empty, button, icon, noButtonText, yesButtonText, cancelButtonText,
            checkBoxText);
    }

    private static string GetActualButtonText(string? value, string defaultValue)
    {
        return (value.Coalesce(defaultValue) ?? string.Empty).GetLocalizedString();
    }

    internal static IServiceCollection AddPopups(this IServiceCollection serviceCollection)
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