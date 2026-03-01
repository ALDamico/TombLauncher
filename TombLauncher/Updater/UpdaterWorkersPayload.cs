using System;
using System.Windows.Input;
using Material.Icons;
using NetSparkleUpdater.AppCastHandlers;
using NetSparkleUpdater.Interfaces;

namespace TombLauncher.Updater;

public class UpdaterWorkersPayload
{
    public IAppCastDataDownloader AppCastDataDownloader { get; set; } = null!;
    public IUpdateDownloader UpdateDownloader { get; set; } = null!;
    public ILogger LoggerToUse { get; set; } = null!;
    public AppCastHelper AppCastHelper { get; set; } = null!;
    public Func<IUIFactory> UiFactory { get; set; } = null!;
    public ICommand UpdateCommand { get; set; } = null!;
    public MaterialIconKind UpdateIcon { get; set; }
}