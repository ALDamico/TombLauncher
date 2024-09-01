using System.IO;
using System.Text.Json;
using System.Windows.Input;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.Input;
using JamSoft.AvaloniaUI.Dialogs;
using JamSoft.AvaloniaUI.Dialogs.ViewModels;
using TombLauncher.Dto;
using TombLauncher.Extensions;
using TombLauncher.Utils;

namespace TombLauncher.ViewModels;

public class AppCrashHostViewModel : DialogViewModel
{
    public AppCrashHostViewModel(IDialogService dialogService)
    {
        _dialogService = dialogService;
        AcceptCommandText = "Accept".GetLocalizedString();
        CopyCmd = new RelayCommand<object>(Copy, CanCopy);
        SaveCmd = new RelayCommand(Save);
        // TODO Hide cancel button when functionality becomes available
    }

    private readonly IDialogService _dialogService;

    private bool CanCopy(object obj)
    {
        return AppUtils.GetClipboard() != null;
    }

    private AppCrashDto _crash;

    public AppCrashDto Crash
    {
        get => _crash;
        set => RaiseAndSetIfChanged(ref _crash, value);
    }
    public override bool CanCancel() => false;

    public ICommand SaveCmd
    {
        get;
    }

    private async void Save()
    {
        var filePath = await _dialogService.SaveFile("Save error details".GetLocalizedString(),
            new FilePickerFileType[] { new FilePickerFileType("JSON files".GetLocalizedString()){Patterns = ["*.json"]} }, "json");
        if (string.IsNullOrWhiteSpace(filePath)) return;
        await File.WriteAllTextAsync(filePath, JsonSerializer.Serialize(Crash, new JsonSerializerOptions(){WriteIndented = true}));
    }
    
    public ICommand CopyCmd { get; }

    private void Copy(object param)
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
}