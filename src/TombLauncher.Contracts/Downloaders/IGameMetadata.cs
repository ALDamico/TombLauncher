namespace TombLauncher.Contracts.Downloaders;

public interface IGameMetadata : IGameMetadataLite
{
    byte[] TitlePic { get; set; }
    List<IEnvironmentVariable> ExtraEnvVars { get; set; }
}