using TombLauncher.Patchers.Gameflows;

namespace TombLauncher.Patchers.Extensions;

public static class SequenceOpcodeExtensions
{
    public static bool RequiresArgument(this SequenceOpcode opcode)
    {
        return opcode switch
        {
            SequenceOpcode.Fmv => true,
            SequenceOpcode.Level => true,
            SequenceOpcode.Cine => true,
            SequenceOpcode.Demo => true,
            SequenceOpcode.JumpToSequence => true,
            SequenceOpcode.Track => true,
            SequenceOpcode.LoadPic => true,
            SequenceOpcode.CutAngle => true,
            SequenceOpcode.NoFloor => true,
            SequenceOpcode.StartInv => true,
            SequenceOpcode.StartAnim => true,
            SequenceOpcode.Secrets => true,
            _ => false
        };
    }
}