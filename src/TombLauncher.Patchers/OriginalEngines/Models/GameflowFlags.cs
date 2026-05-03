namespace TombLauncher.Patchers.OriginalEngines.Models;

// For documentation regarding this, see https://opentomb.github.io/TRosettaStone3/trosettastone.html#_script_flags
[Flags]
public enum GameflowFlags
{
    None = 0,
    DemoVersion = 0x01,
    TitleDisabled = 0x02,
    CheatModeCheckDisabled = 0x04,
    NoInputTimeout = 0x08,
    LoadSaveDisabled = 0x10,
    ScreenSizingDisabled = 0x20,
    LockoutOptionRing = 0x40,
    DozyCheatEnabled = 0x80,
    UseXor = 0x100,
    GymEnabled = 0x200,
    SelectAnyLevel = 0x400,
    EnableCheatCode = 0x800
}