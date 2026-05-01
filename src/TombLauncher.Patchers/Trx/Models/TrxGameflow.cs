namespace TombLauncher.Patchers.Trx.Models;

public class TrxGameflow
{
    public int? Engine { get; set; }
    public string MainMenuPicture { get; set; } = "";
    public required string SavegameFileFmt { get; set; } = "";
    public List<string> Injections { get; set; } = [];
    public bool RequireTr2ItemDrops { get; set; }
    public bool ConvertDroppedGuns { get; set; }
    public Level Title { get; set; }
    public List<Level> Levels { get; set; } = [];
    public List<Level> Demos { get; set; } = [];
    public List<Level> Cutscenes { get; set; } = [];
    public List<SequenceItem> Fmvs { get; set; } = [];
    public List<string> HiddenConfigs { get; set; } = [];
}