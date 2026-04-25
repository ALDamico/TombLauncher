namespace TombLauncher.Configuration.Sections;

public interface IUpdaterConfig
{
    string? AppCastUrl { get; }
    string? AppCastPublicKey { get; }
    bool UpdaterUseLocalPaths { get; }
    string? UpdateChannelName { get; }
    string? GitHubRepositoryOwner { get; }
    string? GitHubRepositoryName { get; }
}