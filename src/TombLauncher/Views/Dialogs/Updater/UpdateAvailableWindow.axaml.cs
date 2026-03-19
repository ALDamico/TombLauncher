#pragma warning disable AVLN3001
using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Threading;
using NetSparkleUpdater;
using NetSparkleUpdater.Enums;
using NetSparkleUpdater.Events;
using NetSparkleUpdater.Interfaces;
using TombLauncher.Contracts.Localization;
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

namespace TombLauncher.Views.Dialogs.Updater;

public partial class UpdateAvailableWindow : Window, IUpdateAvailable
{
    private readonly ILocalizationManager _localization;

    public event UserRespondedToUpdate? UserResponded;

    public UpdateAvailableResult Result { get; private set; } = UpdateAvailableResult.None;

    public AppCastItem CurrentItem => _items.FirstOrDefault()!;

    private readonly List<AppCastItem> _items;

    [Obsolete("Design-time / Avalonia runtime loader only. Use the overload with List<AppCastItem> and ILocalizationManager.")]
    public UpdateAvailableWindow()
    {
        
    }

    public UpdateAvailableWindow(List<AppCastItem> items, ILocalizationManager localization)
    {
        _items = items;
        _localization = localization;
        InitializeComponent();

        var version = items.FirstOrDefault()?.Version ?? string.Empty;
        VersionSubtitle.Text = _localization.GetLocalizedString("NEW_VERSION_AVAILABLE", version);

        var releaseNotes = string.Join("\n\n---\n\n",
            items.Select(i => i.Description ?? string.Empty)
                 .Where(d => !string.IsNullOrWhiteSpace(d)));
        ReleaseNotesMd.Markdown = releaseNotes;

        SkipButton.Click += (_, _) => Respond(UpdateAvailableResult.SkipUpdate);
        RemindLaterButton.Click += (_, _) => Respond(UpdateAvailableResult.RemindMeLater);
        InstallButton.Click += (_, _) => Respond(UpdateAvailableResult.InstallUpdate);
    }

    public void Show(bool isOnMainThread)
    {
        if (isOnMainThread)
            Show();
        else
            Dispatcher.UIThread.Post(Show);
    }

    public void HideReleaseNotes() => ReleaseNotesMd.IsVisible = false;
    public void HideRemindMeLaterButton() => RemindLaterButton.IsVisible = false;
    public void HideSkipButton() => SkipButton.IsVisible = false;

    public void BringToFront()
    {
        if (Dispatcher.UIThread.CheckAccess())
            Activate();
        else
            Dispatcher.UIThread.Post(Activate);
    }

    public new void Close()
    {
        if (Dispatcher.UIThread.CheckAccess())
            base.Close();
        else
            Dispatcher.UIThread.Post(base.Close);
    }

    private void Respond(UpdateAvailableResult result)
    {
        Result = result;
        base.Close();
        UserResponded?.Invoke(this, new UpdateResponseEventArgs(result, CurrentItem));
    }
}
