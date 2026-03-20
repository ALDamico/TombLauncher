using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.DependencyInjection;
using NetSparkleUpdater;
using NetSparkleUpdater.Interfaces;
using TombLauncher.Contracts.Localization;
using TombLauncher.Views.Dialogs.Updater;

namespace TombLauncher.Updater;

/// <summary>
/// NetSparkle UI factory that creates Avalonia windows styled to match TombLauncher's design.
/// Only <see cref="CreateUpdateAvailableWindow"/> and <see cref="CreateProgressWindow"/> produce
/// visible UI; all other methods are silent no-ops because update notifications are surfaced
/// via the app's own <see cref="Services.NotificationService"/>.
/// </summary>
[SuppressMessage("ReSharper", "InconsistentNaming")]
public class TombLauncherUIFactory : IUIFactory
{
    // ── IUIFactory properties (used by NetSparkle to configure the windows) ──
    public bool HideReleaseNotes { get; set; }
    public bool HideSkipButton { get; set; }
    public bool HideRemindMeLaterButton { get; set; }
    public string? ReleaseNotesHTMLTemplate { get; set; }
    public string? AdditionalReleaseNotesHeaderHTML { get; set; }

    private ILocalizationManager Localization =>
        Ioc.Default.GetRequiredService<ILocalizationManager>();

    // ── Window factory methods ────────────────────────────────────────────────

    public IUpdateAvailable CreateUpdateAvailableWindow(
        List<AppCastItem> updates,
        ISignatureVerifier? signatureVerifier,
        string appName,
        string appVersion,
        bool isUpdateAlreadyDownloaded = false)
    {
        UpdateAvailableWindow? window = null;
        Dispatcher.UIThread.Invoke(() =>
            window = new UpdateAvailableWindow(updates, Localization));

        if (HideReleaseNotes) window!.HideReleaseNotes();
        if (HideSkipButton) window!.HideSkipButton();
        if (HideRemindMeLaterButton) window!.HideRemindMeLaterButton();

        return window!;
    }

    public IDownloadProgress CreateProgressWindow(string downloadTitle, string downloadMessage)
    {
        DownloadProgressWindow? window = null;
        Dispatcher.UIThread.Invoke(() =>
            window = new DownloadProgressWindow(Localization));
        return window!;
    }

    // ── Silent no-ops ─────────────────────────────────────────────────────────

    public ICheckingForUpdates ShowCheckingForUpdates() => new SilentCheckingForUpdates();

    public void ShowUnknownInstallerFormatMessage(string downloadFileName) { }

    public void ShowVersionIsUpToDate() { }

    public void ShowVersionIsSkippedByUserRequest() { }

    public void ShowCannotDownloadAppcast(string? appcastUrl) { }

    public bool CanShowToastMessages() => false;

    public void ShowToast(Action clickHandler) { }

    public void ShowDownloadErrorMessage(string message, string? appcastUrl) { }

    public void Shutdown() { }

    // ── Inner helper ──────────────────────────────────────────────────────────

    private sealed class SilentCheckingForUpdates : ICheckingForUpdates
    {
        public event EventHandler? UpdatesUIClosing;
        public void Show() { }
        public void Close() => UpdatesUIClosing?.Invoke(this, EventArgs.Empty);
    }
}
