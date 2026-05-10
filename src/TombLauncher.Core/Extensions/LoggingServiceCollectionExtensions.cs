using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;
using Serilog.Filters;

namespace TombLauncher.Core.Extensions;

public static class LoggingServiceCollectionExtensions
{
    public static IServiceCollection AddTombLauncherLogging(this IServiceCollection services, string appDataDirectory)
    {
        var logPath = Path.Combine(appDataDirectory, "Logs", "TombLauncher_App.log");
        var aiLogPath = Path.Combine(appDataDirectory, "Logs", "TombLauncher_Ai.log");
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .WriteTo.Logger(lc => lc
                .Filter.ByExcluding(Matching.FromSource("TombLauncher.Ai"))
                .WriteTo.File(logPath, LogEventLevel.Information, rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: 7, fileSizeLimitBytes: 10 * 1024 * 1024, rollOnFileSizeLimit: true))
            .WriteTo.Logger(lc => lc
                .Filter.ByIncludingOnly(Matching.FromSource("TombLauncher.Ai"))
                .WriteTo.File(aiLogPath, LogEventLevel.Information, rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: 7, fileSizeLimitBytes: 10 * 1024 * 1024, rollOnFileSizeLimit: true))
            .CreateLogger();

        return services.AddLogging();
    }

    private static IServiceCollection AddLogging(this IServiceCollection services)
    {
        return services.AddLogging(opts =>
        {
            opts.ClearProviders();
            opts.SetMinimumLevel(LogLevel.Information);
            opts.AddSerilog();
        });
    }

    public static IServiceCollection AddEmbedderLogging(this IServiceCollection services)
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .WriteTo
            .Console()
            .CreateLogger();
        AddLogging(services);
        return services;
    }
}