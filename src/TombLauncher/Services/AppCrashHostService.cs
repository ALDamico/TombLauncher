using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Avalonia.Platform.Storage;
using TombLauncher.Contracts.Localization;
using TombLauncher.Core.Dtos;
using TombLauncher.Data.Database.Services;
using TombLauncher.Localization.Extensions;
using TombLauncher.Utils;

namespace TombLauncher.Services;

public class AppCrashHostService : IViewService
{
    public AppCrashHostService(ViewServiceContext viewContext, AppCrashDataService appCrashDataService)
    {
        ViewContext = viewContext;
        AppCrashDataService = appCrashDataService;
    }
    public ViewServiceContext ViewContext { get; }
    public AppCrashDataService AppCrashDataService { get; }
    public ILocalizationManager LocalizationManager => ViewContext.LocalizationManager;
    public NavigationManager NavigationManager => ViewContext.NavigationManager;

    public async Task Save(AppCrashDto crash)
    {
        var filePath = await ViewContext.PopupService.SaveFile("SAVE_ERROR_DETAILS".GetLocalizedString(),
            new FilePickerFileType[]
            {
                new FilePickerFileType("JSON_FILES".GetLocalizedString())
                {
                    Patterns = new string[]
                    {
                        "*.json"
                    }
                }
            }, "json");
        if (string.IsNullOrWhiteSpace(filePath)) return;
        await File.WriteAllTextAsync(filePath,
            JsonSerializer.Serialize(crash, new JsonSerializerOptions() { WriteIndented = true }) ?? string.Empty);
    }

    public void Copy(object param)
    {
        string serialized;
        try
        {
            serialized = JsonSerializer.Serialize(param) ?? string.Empty;
        }
        catch (JsonException)
        {
            serialized = param?.ToString() ?? string.Empty;
        }

        AppUtils.SetClipboardTextAsync(serialized);
    }

    public async Task MarkAsNotified(AppCrashDto crash)
    {
        await MarkAsNotified(crash.Id);
    }

    public async Task MarkAsNotified(int id)
    {
        await AppCrashDataService.MarkAsNotified(id);
    }
}