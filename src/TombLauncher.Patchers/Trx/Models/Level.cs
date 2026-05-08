namespace TombLauncher.Patchers.Trx.Models;

public class Level
{
    public string Path { get; set; }
    public int MusicTrack { get; set; }
    public bool InheritInjections { get; set; }
    public List<SequenceItem> Sequence { get; set; } = [];
    public List<string> Injections { get; set; } = [];
}