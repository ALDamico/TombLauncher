using TombLauncher.Patchers.OriginalEngines.Models;

namespace TombLauncher.Patchers.Shared.Models;

public class GameflowDto
{
    public uint Version { get; set; }
    public string? Description { get; set; }
    public ushort GameflowSize { get; set; }
    public int FirstOption { get; set; }
    public int TitleReplace { get; set; }
    public int OnDeathDemoMode { get; set; }
    public int OnDeathInGame { get; set; }
    public TimeSpan DemoTimeout { get; set; }
    public int OnDemoInterrupt { get; set; }
    public int OnDemoEnd { get; set; }
    public ushort NumLevels { get; set; }
    public ushort NumChapterScreens { get; set; }
    public ushort NumTitles { get; set; }
    public ushort NumFmvs { get; set; }
    public ushort NumCutscenes { get; set; }
    public ushort NumDemoLevels { get; set; }
    public ushort TitleSoundId { get; set; }
    public bool IsSingleLevel { get; set; }
    public GameflowFlags Flags { get; set; }
    public byte XorKey { get; set; }
    public GameflowLanguage LanguageId { get; set; }
    public ushort SecretSoundId { get; set; }
    public List<LevelInfo> Levels { get; set; } = [];
}