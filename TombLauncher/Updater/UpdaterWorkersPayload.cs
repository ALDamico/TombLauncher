using NetSparkleUpdater.Interfaces;

namespace TombLauncher.Updater;

public class UpdaterWorkersPayload
{
    public IAppCastDataDownloader AppCastDataDownloader { get; set; }
    public IUpdateDownloader UpdateDownloader { get; set; }
    public ILogger LoggerToUse { get; set; }
}