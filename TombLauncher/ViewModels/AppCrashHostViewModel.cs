using System.Text.Json;
using System.Windows.Input;
using Avalonia.Controls.ApplicationLifetimes;
using AvaloniaEdit;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TombLauncher.Dto;
using TombLauncher.Extensions;

namespace TombLauncher.ViewModels;

public partial class AppCrashHostViewModel : DialogViewModelBase
{
    public AppCrashHostViewModel()
    {
        AcceptCommandText = "Accept".GetLocalizedString();
        CopyCmd = new RelayCommand<object>(Copy, CanCopy);
        //CancelCommandText = "Annulla";
    }

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