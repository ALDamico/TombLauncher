using TombLauncher.Configuration.Sections;

namespace TombLauncher.Configuration;

public interface ILayeredAppConfiguration : IAppConfiguration
{
    AppConfiguration Defaults { get; }
    AppConfiguration User { get; }
}