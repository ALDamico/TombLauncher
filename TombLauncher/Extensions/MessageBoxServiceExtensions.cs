using System.Threading.Tasks;
using JamSoft.AvaloniaUI.Dialogs;
using JamSoft.AvaloniaUI.Dialogs.MsgBox;
using TombLauncher.Core.Utils;
using TombLauncher.Localization.Extensions;

namespace TombLauncher.Extensions;

public static class MessageBoxServiceExtensions
{
    public static Task<MsgBoxResult> ShowLocalized(this IMessageBoxService service, string caption, string messageBoxText, MsgBoxButton button,
        MsgBoxImage icon = MsgBoxImage.None, string noButtonText = null, string yesButtonText = null,
        string cancelButtonText = null, string checkBoxText = null)
    {
        noButtonText = GetActualButtonText(noButtonText, "No");
        yesButtonText = GetActualButtonText(yesButtonText, "Yes");
        cancelButtonText = GetActualButtonText(cancelButtonText, "Cancel");
        checkBoxText = checkBoxText?.GetLocalizedString();
        caption = caption?.GetLocalizedString();
        messageBoxText = messageBoxText?.GetLocalizedString();

        return service.Show(caption, messageBoxText, button, icon, noButtonText, yesButtonText, cancelButtonText,
            checkBoxText);
    }

    private static string GetActualButtonText(string value, string defaultValue)
    {
        return value.Coalesce(defaultValue).GetLocalizedString();
    }
}