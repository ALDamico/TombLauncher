using System.IO;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;

namespace TombLauncher.Extensions;

public static class LoggingServiceCollectionExtensions
{
    public static IServiceCollection AddTombLauncherLogging(this IServiceCollection services, string appDataDirectory)
    {
        var logPath = Path.Combine(appDataDirectory, "TombLauncher_App.log");
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .WriteTo
            .File(logPath, LogEventLevel.Information)
            .CreateLogger();
        services.AddLogging(opts =>
        {
            opts.ClearProviders();
            opts.SetMinimumLevel(LogLevel.Information);
            opts.AddSerilog();
        });
        return services;
    }
}
