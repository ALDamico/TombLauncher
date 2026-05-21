using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.DependencyInjection;
using ICSharpCode.SharpZipLib;
using TombLauncher.Contracts.Downloaders;
using TombLauncher.Contracts.PlatformSpecific;
using TombLauncher.Contracts.Progress;
using TombLauncher.Contracts.Settings;
using TombLauncher.Core.PlatformSpecific;
using TombLauncher.Core.Utils;
using TombLauncher.Services;

namespace TombLauncher.Installers;

public class TombRaiderLevelInstaller
{
    private readonly IPlatformSpecificFeatures _platformSpecificFeatures;

    public TombRaiderLevelInstaller(IPlatformSpecificFeatures platformSpecificFeatures)
    {
        _platformSpecificFeatures = platformSpecificFeatures;
    }

    public async Task<string> Install(string containingFolder, IGameMetadata gameDto, CancellationToken cancellationToken,
        IProgress<CopyProgressInfo>? copyProgress = null)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (!Directory.Exists(containingFolder) && !File.Exists(containingFolder))
        {
            throw new ArgumentException("The source folder does not exist!", nameof(containingFolder));
        }
        var installFolder =
            PathUtils.GetGameInstallFolder(_platformSpecificFeatures.GetAppDataDirectory(), gameDto.Guid);

        if (Directory.Exists(containingFolder))
        {
            CopyDirectory(containingFolder, installFolder, true);
        }
        else if (File.Exists(containingFolder))
        {
            using var zipManager = new ZipManager(containingFolder);
            try
            {
                await zipManager.ExtractAll(installFolder, cancellationToken, copyProgress);
            }
            catch (SharpZipBaseException)
            {
                // Something happened that doesn't allow us to use SharpZipLib.
                // Let's fallback to a simpler method
                var settingsProvider = Ioc.Default.GetRequiredService<ISettingsProvider>();
                var commandLineToExecute = settingsProvider.GetGameDetailsSettings().UnzipFallbackMethodCommandLine;
                var targetDirectory = PathUtils.GetRandomTempDirectory();
                var commandLineArguments = string.Format(commandLineToExecute.CommandLineArguments, containingFolder, targetDirectory);
                var process = Process.Start(commandLineToExecute.Command, commandLineArguments);
                await process.WaitForExitAsync();
                return await Install(targetDirectory, gameDto, cancellationToken, copyProgress);
            }
        }

        return installFolder;
    }

    private static void CopyDirectory(string sourceDir, string destinationDir, bool recursive, IProgress<CopyProgressInfo>? progress = null)
    {
        // Get information about the source directory
        var dir = new DirectoryInfo(sourceDir);

        // Check if the source directory exists
        if (!dir.Exists)
            throw new DirectoryNotFoundException($"Source directory not found: {dir.FullName}");

        // Cache directories before we start copying
        var dirs = dir.GetDirectories();

        // Create the destination directory
        Directory.CreateDirectory(destinationDir);

        if (progress != null)
        {
            var copyProgressInfo = new CopyProgressInfo()
            {
                CurrentFileName = dir.Name
            };
            progress.Report(copyProgressInfo);
        }

        // Get the files in the source directory and copy to the destination directory
        foreach (var file in dir.GetFiles())
        {
            var targetFilePath = Path.Combine(destinationDir, file.Name);
            if (progress != null)
            {
                var copyProgressInfo = new CopyProgressInfo()
                {
                    CurrentFileName = Path.Combine(dir.FullName.Replace(destinationDir, string.Empty), file.Name)
                };
                progress.Report(copyProgressInfo);
            }
            file.CopyTo(targetFilePath);
        }

        // If recursive and copying subdirectories, recursively call this method
        if (!recursive) return;
        foreach (var subDir in dirs)
        {
            var newDestinationDir = Path.Combine(destinationDir, subDir.Name);
            CopyDirectory(subDir.FullName, newDestinationDir, true);
        }
    }
}