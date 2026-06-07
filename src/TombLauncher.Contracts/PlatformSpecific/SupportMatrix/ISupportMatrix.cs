using TombLauncher.Contracts.Enums;
using TombLauncher.Contracts.PlatformSpecific.Models;

namespace TombLauncher.Contracts.PlatformSpecific.SupportMatrix;

public interface ISupportMatrix
{
    IReadOnlyDictionary<GameEngine, GameSupportMatrixEntry> Matrix { get; }

    EngineSupportState GetEngineSupportState(GameEngine? engine)
    {
        if (engine == null)
            return EngineSupportState.NoSupport;
        
        var value = Matrix.GetValueOrDefault(engine.Value, null);
        if (value == null)
            return EngineSupportState.NoSupport;

        return value.SupportState;
    }

    IEnumerable<GameSupportMatrixEntry> GetEntries() => Matrix.Values;
}