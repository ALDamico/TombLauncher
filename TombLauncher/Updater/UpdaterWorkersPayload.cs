using System;
using System.Windows.Input;
using Material.Icons;
using NetSparkleUpdater.AppCastHandlers;
using NetSparkleUpdater.Interfaces;

namespace TombLauncher.Updater;

public class UpdaterWorkersPayload
{
    public IAppCastDataDownloader AppCastDataDownloader { get; set; }
    public IUpdateDownloader UpdateDownloader { get; set; }
    public ILogger LoggerToUse { get; set; }
    public AppCastHelper AppCastHelper { get; set; }
    public Func<IUIFactory> UiFactory { get; set; }
    public ICommand UpdateCommand { get; set; }
    public MaterialIconKind UpdateIcon { get; set; }
}