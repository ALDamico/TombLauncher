using System.IO;
using System.Text.Json;
using System.Windows.Input;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using AvaloniaEdit;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using JamSoft.AvaloniaUI.Dialogs;
using TombLauncher.Dto;
using TombLauncher.Extensions;

namespace TombLauncher.ViewModels;

public partial class AppCrashHostViewModel : DialogViewModelBase
{
    public AppCrashHostViewModel(IDialogService dialogService)
    {
        _dialogService = dialogService;
        AcceptCommandText = "Accept".GetLocalizedString();
        CopyCmd = new RelayCommand<object>(Copy, CanCopy);
        SaveCmd = new RelayCommand(Save);
        //CancelCommandText = "Annulla";
    }

    private readonly IDialogService _dialogService;

    private bool CanCopy(object obj)
    {
        var applicationLifetime = App.Current.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime;
        var clipboard = applicationLifetime?.MainWindow?.Clipboard;
        return clipboard != null;
    }

    [ObservableProperty] private AppCrashDto _crash;
    protected override void Accept()
    {
        base.Accept();
    }

    protected override bool CanAcceptInner()
    {
        return true;
    }

    protected override void Cancel()
    {
        
    }

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

        var applicationLifetime = App.Current.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime;
        var clipboard = applicationLifetime?.MainWindow?.Clipboard;
        clipboard?.SetTextAsync(serialized);
    }
}