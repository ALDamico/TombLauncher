using System.ComponentModel;

namespace TombLauncher.Patchers.Tomb4Plus.Enums;

public enum GradientType
{
    Normal,
    GradientTr5,
    GradientFlat
}

public enum PatchBinaryType
{
    TrepExe = 1,
    FlepExe = 2,
    FlepExternalBinary = 3
}

public enum SyntaxFile
{
    [Description("Early TRLE syntax")]
    Early,
    [Description("Common TREP syntax")]
    Trep,
    [Description("TRLE scripting via Lua")]
    TrLarson,
}