using TombLauncher.Patchers.OriginalEngines.Models;

namespace TombLauncher.Patchers.Trx.Models;

public class SequenceItem
{
    private const double DisplayTimeDefault = 5.0;
    private const double FadeInTimeDefault = 1.0;
    private const double FadeOutTimeDefault = 0.33;
    private SequenceItem() {}
    public required string Type { get; set; }
    public string? Path { get; set; }
    public bool? Legal { get; set; }
    public double? DisplayTime { get; set; }
    public double? FadeInTime { get; set; }
    public double? FadeOutTime { get; set; }

    public static SequenceItem LoopGame { get; } = new SequenceItem()
    {
        Type = "loop_game",
    };

    public static SequenceItem LevelComplete { get; } = new SequenceItem() { Type = "level_complete" };
    public static SequenceItem ExitToTitle { get; } = new SequenceItem() { Type = "exit_to_title" };
    public static SequenceItem LevelStats { get; } = new SequenceItem() { Type = "level_stats" };

    public static SequenceItem TotalStats(string backgroundPicture)
    {
        return new SequenceItem() { Type = "total_stats", Path = backgroundPicture };
    }

    public static SequenceItem GlobeSelect(object globeSelectEntries, string image) =>
        throw new NotImplementedException();

    public static SequenceItem DisplayPicture(string path, double displayTime = DisplayTimeDefault,
        double fadeInTime = FadeInTimeDefault, double fadeOutTime = FadeOutTimeDefault)
    {
        return new SequenceItem()
        {
            Type = "display_picture",
            Path = path,
            DisplayTime = displayTime,
            FadeInTime = fadeInTime,
            FadeOutTime = fadeOutTime
        };
    }

    public static SequenceItem LoadingScreen(string path, double displayTime = DisplayTimeDefault,
        double fadeInTime = FadeInTimeDefault, double fadeOutTime = FadeOutTimeDefault)
    {
        return new SequenceItem()
        {
            Type = "loading_screen",
            Path = path,
            DisplayTime = displayTime,
            FadeInTime = fadeInTime,
            FadeOutTime = fadeOutTime
        };
    }
}