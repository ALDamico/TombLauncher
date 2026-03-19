#pragma warning disable AVLN3001
using System;
using Avalonia.Controls;
using Avalonia.Threading;
using NetSparkleUpdater.Events;
using NetSparkleUpdater.Interfaces;
using TombLauncher.Contracts.Localization;

namespace TombLauncher.Views.Dialogs.Updater;

public partial class DownloadProgressWindow : Window, IDownloadProgress
{
    private readonly ILocalizationManager _localization;

    public event DownloadInstallEventHandler? DownloadProcessCompleted;

    [Obsolete("Design-time / Avalonia runtime loader only. Use the overload with ILocalizationManager.")]
    public DownloadProgressWindow() : this(null!) { }

    public DownloadProgressWindow(ILocalizationManager localization)
    {
        _localization = localization;
        InitializeComponent();

        CancelButton.Click += (_, _) =>
        {
            DownloadProcessCompleted?.Invoke(this, new DownloadInstallEventArgs(false));
            base.Close();
        };

        InstallButton.Click += (_, _) =>
        {
            DownloadProcessCompleted?.Invoke(this, new DownloadInstallEventArgs(true));
            base.Close();
        };
    }

    public void SetDownloadAndInstallButtonEnabled(bool shouldBeEnabled)
    {
        Dispatcher.UIThread.Post(() => InstallButton.IsEnabled = shouldBeEnabled);
    }

    public void Show(bool isOnMainThread)
    {
        if (isOnMainThread)
            Show();
        else
            Dispatcher.UIThread.Post(Show);
    }

    public void OnDownloadProgressChanged(object sender, ItemDownloadProgressEventArgs args)
    {
        Dispatcher.UIThread.Post(() =>
        {
            ProgressBar.Value = args.ProgressPercentage;
            ProgressLabel.Text = $"{args.ProgressPercentage}%";
        });
    }

    public new void Close()
    {
        if (Dispatcher.UIThread.CheckAccess())
            base.Close();
        else
            Dispatcher.UIThread.Post(base.Close);
    }

    public void FinishedDownloadingFile(bool isDownloadedFileValid)
    {
        Dispatcher.UIThread.Post(() =>
        {
            Spinner.IsVisible = false;
            ProgressBar.Value = 100;

            if (isDownloadedFileValid)
            {
                StatusLabel.Text = _localization.GetLocalizedString("INSTALL_AND_RELAUNCH");
                ProgressLabel.Text = string.Empty;
                CancelButton.IsVisible = false;
                InstallButton.IsVisible = true;
            }
            else
            {
                ErrorLabel.Text = _localization.GetLocalizedString("UPDATE_SIGNATURE_VERIFY_ERROR");
                ErrorLabel.IsVisible = true;
                CancelButton.IsVisible = true;
            }
        });
    }

    public bool DisplayErrorMessage(string errorMessage)
    {
        Dispatcher.UIThread.Post(() =>
        {
            ErrorLabel.Text = errorMessage;
            ErrorLabel.IsVisible = true;
            Spinner.IsVisible = false;
        });
        return true;
    }
}
