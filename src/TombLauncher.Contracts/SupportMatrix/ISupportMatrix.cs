using TombLauncher.Contracts.Enums;

namespace TombLauncher.Contracts.SupportMatrix;

public interface ISupportMatrix
{
    IReadOnlyDictionary<GameEngine, EngineSupportState> Matrix { get; }
    EngineSupportState GetEngineSupportState(GameEngine engine) => Matrix.GetValueOrDefault(engine, EngineSupportState.NoSupport);
}