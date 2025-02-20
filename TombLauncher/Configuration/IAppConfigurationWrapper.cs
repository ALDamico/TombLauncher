namespace TombLauncher.Configuration;

public interface IAppConfigurationWrapper : IAppConfiguration
{
    public IAppConfiguration Defaults { get; set; }
    public IAppConfiguration User { get; set; }
}