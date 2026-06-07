using TombLauncher.Contracts.Enums;

namespace TombLauncher.Contracts.SupportMatrix;

public interface ISupportMatrix
{
    IReadOnlyDictionary<GameEngine, EngineSupportState> Matrix { get; }

    EngineSupportState GetEngineSupportState(GameEngine? engine)
    {
        if (engine == null)
            return EngineSupportState.NoSupport;
        
        return Matrix.GetValueOrDefault(engine.Value, EngineSupportState.NoSupport);
    }
}