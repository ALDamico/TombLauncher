using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TombLauncher.Core.Utils;

namespace TombLauncher.Services;

public class AppFileOperationsService : IAppFileOperationsService
{
    private readonly ILogger<AppFileOperationsService> _logger;

    public AppFileOperationsService(ILogger<AppFileOperationsService> logger)
    {
        _logger = logger;
    }

    public Task CleanUpTempFiles()
    {
        _logger.LogInformation("Starting temp folder clean up");
        var tempFolder = PathUtils.GetTombLauncherTempDirectory();

        if (!Directory.Exists(tempFolder))
            return Task.CompletedTask;

        var entries = Directory.GetFileSystemEntries(tempFolder).Where(e => e != tempFolder).ToArray();
        foreach (var entry in entries)
        {
            try
            {
                _logger.LogInformation("Deleting entry {EntryName}", entry);
                if (Directory.Exists(entry))
                {
                    Directory.Delete(entry, true);
                }
                else if (File.Exists(entry))
                {
                    File.Delete(entry);
                }
            }
            catch (IOException ex)
            {
                _logger.LogError("An IOException was ignored: {Ex}", ex);
            }
        }

        _logger.LogInformation("Temp folder clean up completed");
        return Task.CompletedTask;
    }

    public Task DeleteDirectory(string path)
    {
        return Task.Run(() =>
        {
            if (string.IsNullOrWhiteSpace(path) || !Directory.Exists(path))
                return;

            try
            {
                Directory.Delete(path, true);
            }
            catch (IOException ex)
            {
                _logger.LogError("An IOException was ignored while deleting directory {Path}: {Ex}", path, ex);
            }
        });
    }

    public Task DeleteFile(string path)
    {
        return Task.Run(() =>
        {
            if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
                return;

            try
            {
                File.Delete(path);
            }
            catch (IOException ex)
            {
                _logger.LogError("An IOException was ignored while deleting file {Path}: {Ex}", path, ex);
            }
        });
    }
}
