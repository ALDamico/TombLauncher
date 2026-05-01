using TombLauncher.Patchers.OriginalEngines;

namespace TombLauncher.Patchers.OriginalEngines.Models;

public class Gameflow
{
    public uint Version { get; set; }
    public string? Description { get; set; }
    public ushort GameflowSize { get; set; }
    public int FirstOption { get; set; }
    public int TitleReplace { get; set; }
    public int OnDeathDemoMode { get; set; }
    public int OnDeathInGame { get; set; }
    public uint DemoTime { get; set; }
    public int OnDemoInterrupt { get; set; }
    public int OnDemoEnd { get; set; }
    public ushort NumLevels { get; set; }
    public ushort NumChapterScreens { get; set; }
    public ushort NumTitles { get; set; }
    public ushort NumFmvs { get; set; }
    public ushort NumCutscenes { get; set; }
    public ushort NumDemoLevels  { get; set; }
    public ushort TitleSoundId { get; set; }
    public ushort SingleLevel { get; set; }
    public GameflowFlags Flags { get; set; }
    public byte XorKey { get; set; }
    public GameflowLanguage LanguageId { get; set; }
    public ushort SecretSoundId { get; set; }
    public TpcStringArray? LevelStrings { get; set; }
    public TpcStringArray? ChapterScreenStrings { get; set; }
    public TpcStringArray? TitleStrings { get; set; }
    public TpcStringArray? FmvStrings { get; set; }
    public TpcStringArray? LevelPathStrings { get; set; }
    public TpcStringArray? CutscenePathStrings { get; set; }
    public ushort[]? SequenceOffsets  { get; set; }
    public ushort SequenceNumBytes { get; set; }
    public Sequence[]? Sequences { get; set; }
    public ushort[]? DemoLevelIds { get; set; }
    public ushort NumGameStrings { get; set; }
    public TpcStringArray? GameStrings { get; set; }
    public TpcStringArray? PcStrings { get; set; }
    public TpcStringArray[] PuzzleStrings { get; } = new TpcStringArray[Constants.NumPuzzleItemsPerLevel];
    public TpcStringArray[] PickupStrings { get;  } = new TpcStringArray[Constants.NumPickupsPerLevel];
    public TpcStringArray[] KeyStrings { get; } = new TpcStringArray[Constants.NumKeysPerLevel];
}