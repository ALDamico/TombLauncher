using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Avalonia.Platform.Storage;
using JamSoft.AvaloniaUI.Dialogs;
using TombLauncher.Contracts.Localization;
using TombLauncher.Data.Database.UnitOfWork;
using TombLauncher.Data.Dto;
using TombLauncher.Localization;
using TombLauncher.Localization.Extensions;
using TombLauncher.Navigation;
using TombLauncher.Utils;

namespace TombLauncher.Services;

public class AppCrashHostService : IViewService
{
    public AppCrashHostService(AppCrashUnitOfWork appCrashUnitOfWork, ILocalizationManager localizationManager, NavigationManager navigationManager, IMessageBoxService messageBoxService, IDialogService dialogService)
    {
        AppCrashUnitOfWork = appCrashUnitOfWork;
        LocalizationManager = localizationManager;
        NavigationManager = navigationManager;
        MessageBoxService = messageBoxService;
        DialogService = dialogService;
    }
    public AppCrashUnitOfWork AppCrashUnitOfWork { get; }
    public ILocalizationManager LocalizationManager { get; }
    public NavigationManager NavigationManager { get; }
    public IMessageBoxService MessageBoxService { get; }
    public IDialogService DialogService { get; }

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
        if (string.IsNullOrWhiteSpace(filePath)) return;
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

    public void MarkAsNotified(AppCrashDto crash)
    {
        AppCrashUnitOfWork.MarkAsNotified(crash.Id);
    }
}