using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Octokit;
using TombLauncher.Contracts.Downloaders;
using TombLauncher.Contracts.Localization;
using TombLauncher.Contracts.Settings;
using TombLauncher.Core.Utils;
using TombLauncher.Installers;
using TombLauncher.Installers.Downloaders;
using TombLauncher.Installers.Downloaders.AspideTR.com;
using TombLauncher.Installers.Downloaders.RaidingTheGlobe.com;
using TombLauncher.Installers.Downloaders.TRCustoms.org;
using TombLauncher.Installers.Downloaders.TRLE.net;
using TombLauncher.Services;

namespace TombLauncher.Extensions;

public static class DownloaderServiceCollectionExtensions
{
    public static IServiceCollection AddDownloaders(this IServiceCollection services)
    {
        var appVersion = VersionUtils.GetApplicationVersion();
        var versionString = appVersion is null ? "0.0.0" : $"{appVersion.Major}.{appVersion.Minor}.{appVersion.Build}";
        services.AddSingleton(new GitHubClient(new Octokit.ProductHeaderValue("TombLauncher", versionString)))
            .AddTransient<GitHubReleaseService>()
            .AddHttpClient(nameof(TrleGameDownloader), c =>
            {
                c.BaseAddress = new Uri("https://trle.net");
                c.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("text/html"));
                c.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/xhtml+xml"));
                c.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/xml"));
                c.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("image/avif"));
                c.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("image/webp"));
                c.DefaultRequestHeaders.Referrer = new Uri("https://trle.net/pFind.php");
            });
        services.AddHttpClient(nameof(AspideTrGameDownloader),
            c => { c.BaseAddress = new Uri("https://www.aspidetr.com/"); });
        services.AddHttpClient(nameof(TrCustomsGameDownloader), c =>
        {
            c.BaseAddress = new Uri("https://trcustoms.org/");
            c.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        });
        services.AddHttpClient(nameof(RaidingTheGlobeGameDownloader),
            c => { c.BaseAddress = new Uri("https://www.raidingtheglobe.com"); });

        services.AddTransient<TrleGameDownloader>()
            .AddTransient(sp => new AspideTrGameDownloader(
                sp.GetRequiredService<IHttpClientFactory>(),
                sp.GetRequiredService<ILocalizationManager>().GetSubsetInvertedByPrefix("ATR"),
                sp.GetRequiredService<ILogger<AspideTrGameDownloader>>()))
            .AddTransient<TrCustomsGameDownloader>()
            .AddTransient<RaidingTheGlobeGameDownloader>();

        // Registrazione come IGameDownloader per GetServices<IGameDownloader>()
        services.AddTransient<IGameDownloader, TrleGameDownloader>()
            .AddTransient<IGameDownloader>(sp => new AspideTrGameDownloader(
                sp.GetRequiredService<IHttpClientFactory>(),
                sp.GetRequiredService<ILocalizationManager>().GetSubsetInvertedByPrefix("ATR"),
                sp.GetRequiredService<ILogger<AspideTrGameDownloader>>()))
            .AddTransient<IGameDownloader, TrCustomsGameDownloader>()
            .AddTransient<IGameDownloader, RaidingTheGlobeGameDownloader>()
            .AddTransient(sp =>
            {
                var downloadManager = new GameDownloadManager(sp.GetRequiredService<IGameMerger>())
                {
                    Downloaders = sp.GetRequiredService<ISettingsProvider>().GetActiveDownloaders()
                };

                return downloadManager;
            })
            .AddTransient(_ => new GameFileHashCalculator(new HashSet<string>()
            {
                ".tr4",
                ".pak",
                ".tr2",
                ".sfx",
                ".dat",
                ".phd"
            }));
        return services;
    }
}