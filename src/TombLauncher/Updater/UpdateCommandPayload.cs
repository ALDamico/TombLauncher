using NetSparkleUpdater;
using NetSparkleUpdater.Events;

namespace TombLauncher.Updater;

public class UpdateCommandPayload
{
    public UpdateCommandPayload(SparkleUpdater sparkle, UpdateDetectedEventArgs args)
    {
        Sparkle = sparkle;
        Args = args;
    }
    public SparkleUpdater Sparkle { get; set; }
    public UpdateDetectedEventArgs Args { get; set; }
}