using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Avalonia.Platform.Storage;
using JamSoft.AvaloniaUI.Dialogs;
using TombLauncher.Contracts.Localization;
using TombLauncher.Core.Dtos;
using TombLauncher.Core.Extensions;
using TombLauncher.Core.Navigation;
using TombLauncher.Data.Database.UnitOfWork;
using TombLauncher.Localization;
using TombLauncher.Localization.Extensions;
using TombLauncher.Utils;

namespace TombLauncher.Services;

public class AppCrashHostService : IViewService
{
    public AppCrashHostService(ViewServiceContext viewContext, AppCrashUnitOfWork appCrashUnitOfWork)
    {
        ViewContext = viewContext;
        AppCrashUnitOfWork = appCrashUnitOfWork;
    }
    public ViewServiceContext ViewContext { get; }
    public AppCrashUnitOfWork AppCrashUnitOfWork { get; }
    public ILocalizationManager LocalizationManager => ViewContext.LocalizationManager;
    public NavigationManager NavigationManager => ViewContext.NavigationManager;
    public IMessageBoxService MessageBoxService => ViewContext.MessageBoxService;
    public IDialogService DialogService => ViewContext.DialogService;

    public async Task Save(AppCrashDto crash)
    {
        var filePath = await DialogService.SaveFile("Save error details".GetLocalizedString(),
            new FilePickerFileType[]
            {
                new FilePickerFileType("JSON files".GetLocalizedString())
                {
                    Patterns = new string[]
                    {
                        "*.json"
                    }
                }
            }, "json");
        if (filePath.IsNullOrWhiteSpace()) return;
        await File.WriteAllTextAsync(filePath,
            JsonSerializer.Serialize(crash, new JsonSerializerOptions() { WriteIndented = true }));
    }

    public void Copy(object param)
    {
        string serialized;
        try
        {
            serialized = JsonSerializer.Serialize(param);
        }
        catch (JsonException)
        {
            serialized = param?.ToString();
        }

        AppUtils.SetClipboardTextAsync(serialized);
    }

    public async Task MarkAsNotified(AppCrashDto crash)
    {
        await MarkAsNotified(crash.Id);
    }

    public async Task MarkAsNotified(int id)
    {
        await AppCrashUnitOfWork.MarkAsNotified(id);
    }
}