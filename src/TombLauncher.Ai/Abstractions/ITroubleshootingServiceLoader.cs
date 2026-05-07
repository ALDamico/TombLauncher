namespace TombLauncher.Ai.Abstractions;

public interface ITroubleshootingServiceLoader
{
    Task<ITroubleshootingService> Load(IProgress<float> progress, CancellationToken cancellationToken);
}