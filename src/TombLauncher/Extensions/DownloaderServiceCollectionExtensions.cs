using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http.Resilience;
using Microsoft.Extensions.Logging;
using Octokit;
using Polly;
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
using TombLauncher.Installers.Resilience;
using TombLauncher.Services;

namespace TombLauncher.Extensions;

public static class DownloaderServiceCollectionExtensions
{
    private const int PerAttemptTimeoutSeconds = 15;
    private const int RetryAttempts = 3;
    private const double RetryBaseDelaySeconds = 2.0;
    private const int CircuitBreakerMinThroughput = 5;
    private const double CircuitBreakerFailureRatio = 0.5;
    private const int CircuitBreakerSamplingSeconds = 30;
    private const int CircuitBreakerBreakSeconds = 60;

    private static IServiceCollection AddDownloaderInfrastructure<T>(this IServiceCollection services,
        Action<HttpClient> configureClient)
    {
        var downloaderName = typeof(T).Name;

        services.AddHttpClient(downloaderName, configureClient)
            .AddHttpMessageHandler(sp => new ResponseTimeMeasurementHandler(downloaderName,
                sp.GetRequiredService<IDownloaderResponseTimeService>()))
            .AddResilienceHandler(downloaderName, builder =>
            {
                builder.AddTimeout(TimeSpan.FromSeconds(PerAttemptTimeoutSeconds));
                builder.AddRetry(new HttpRetryStrategyOptions()
                {
                    MaxRetryAttempts = RetryAttempts,
                    BackoffType = DelayBackoffType.Exponential,
                    Delay = TimeSpan.FromSeconds(RetryBaseDelaySeconds),
                    UseJitter = true
                });

                builder.AddCircuitBreaker(new HttpCircuitBreakerStrategyOptions()
                {
                    MinimumThroughput = CircuitBreakerMinThroughput,
                    FailureRatio = CircuitBreakerFailureRatio,
                    SamplingDuration = TimeSpan.FromSeconds(CircuitBreakerSamplingSeconds),
                    BreakDuration = TimeSpan.FromSeconds(CircuitBreakerBreakSeconds)
                });
            });

        return services;
    }

    public static IServiceCollection AddDownloaders(this IServiceCollection services)
    {
        var appVersion = VersionUtils.GetApplicationVersion();
        var versionString = appVersion is null ? "0.0.0" : $"{appVersion.Major}.{appVersion.Minor}.{appVersion.Build}";
        services.AddSingleton(new GitHubClient(new Octokit.ProductHeaderValue("TombLauncher", versionString)))
            .AddTransient<GitHubReleaseService>()
            .AddDownloaderInfrastructure<TrleGameDownloader>(c =>
            {
                c.BaseAddress = new Uri("https://trle.net");
                c.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("text/html"));
                c.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/xhtml+xml"));
                c.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/xml"));
                c.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("image/avif"));
                c.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("image/webp"));
                c.DefaultRequestHeaders.Referrer = new Uri("https://trle.net/pFind.php");
            })
            .AddDownloaderInfrastructure<AspideTrGameDownloader>(c =>
            {
                c.BaseAddress = new Uri("https://www.aspidetr.com/");
            })
            .AddDownloaderInfrastructure<TrCustomsGameDownloader>(c =>
            {
                c.BaseAddress = new Uri("https://trcustoms.org/");
                c.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            })
            .AddDownloaderInfrastructure<RaidingTheGlobeGameDownloader>(c =>
            {
                c.BaseAddress = new Uri("https://www.raidingtheglobe.com");
            })
            .AddTransient<TrleGameDownloader>()
            .AddTransient(sp => new AspideTrGameDownloader(
                sp.GetRequiredService<IHttpClientFactory>(),
                sp.GetRequiredService<ILocalizationManager>().GetSubsetInvertedByPrefix("ATR"),
                sp.GetRequiredService<ILogger<AspideTrGameDownloader>>()))
            .AddTransient<TrCustomsGameDownloader>()
            .AddTransient<RaidingTheGlobeGameDownloader>()
            // Registrazione come IGameDownloader per GetServices<IGameDownloader>()
            .AddTransient<IGameDownloader, TrleGameDownloader>()
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
            }))
            .AddSingleton<IDownloaderResponseTimeService, DownloaderResponseTimeService>();
        return services;
    }
}