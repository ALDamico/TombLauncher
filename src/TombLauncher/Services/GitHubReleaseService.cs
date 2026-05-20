using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Octokit;
using TombLauncher.Configuration;
using TombLauncher.Contracts.PlatformSpecific;
using TombLauncher.Core.Extensions;
using TombLauncher.Core.PlatformSpecific;
using TombLauncher.Core.Utils;
using TombLauncher.Utils;

namespace TombLauncher.Services;

public class GitHubReleaseService
{
    private readonly GitHubClient _gitHubClient;
    private readonly IPlatformSpecificFeatures _platformSpecificFeatures;
    private readonly IAppConfiguration _appConfiguration;

    public GitHubReleaseService(GitHubClient gitHubClient, IPlatformSpecificFeatures platformSpecificFeatures, IAppConfiguration appConfiguration)
    {
        _gitHubClient = gitHubClient;
        _platformSpecificFeatures = platformSpecificFeatures;
        _appConfiguration = appConfiguration;
    }

    /// <summary>
    /// Returns the Markdown body of the GitHub release matching the current app version.
    /// Checks local cache first; fetches from GitHub only on cache miss.
    /// Returns null on network error with no cache available, or if owner/repo are not configured.
    /// </summary>
    public async Task<string?> GetChangelogMarkdownAsync()
    {
        var repoOwner = _appConfiguration.Updater.GitHubRepositoryOwner;
        var repoName = _appConfiguration.Updater.GitHubRepositoryName;

        if (string.IsNullOrWhiteSpace(repoOwner) || string.IsNullOrWhiteSpace(repoName))
            return null;

        var version = GetAppVersionString();
        var cacheFile = GetCacheFilePath(version);

        if (File.Exists(cacheFile))
        {
            return await File.ReadAllTextAsync(cacheFile);
        }

        try
        {
            var releases = await _gitHubClient.Repository.Release.GetAll(repoOwner, repoName);
            if (releases == null || releases.Count == 0)
                return null;

            var tagToMatch = $"v{version}";
            
            var matched = releases.SingleOrDefault(r => tagToMatch.Equals(r.TagName, StringComparison.OrdinalIgnoreCase)) ?? 
                          releases.OrderByDescending(r => r.PublishedAt).FirstOrDefault();

            if (matched == null)
                return null;

            var body = matched.Body;
            if (body.IsNotNullOrWhiteSpace())
            {
                await File.WriteAllTextAsync(cacheFile, body);
            }

            return body;
        }
        catch
        {
            return null;
        }
    }

    private static string GetAppVersionString()
    {
        var version = VersionUtils.GetApplicationVersion();
        return version is null
            ? "0.0.0"
            : $"{version.Major}.{version.Minor}.{version.Build}";
    }

    private string GetCacheFilePath(string version)
    {
        var appDataDir = _platformSpecificFeatures.GetAppDataDirectory();
        return Path.Combine(appDataDir, $"changelog-{version}.md");
    }
}