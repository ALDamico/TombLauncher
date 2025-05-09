﻿using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.DependencyInjection;
using ICSharpCode.SharpZipLib;
using ICSharpCode.SharpZipLib.Zip;
using TombLauncher.Contracts.Downloaders;
using TombLauncher.Contracts.Progress;
using TombLauncher.Core.Utils;
using TombLauncher.Services;

namespace TombLauncher.Installers;

public class TombRaiderLevelInstaller
{
    public async Task<string> Install(string containingFolder, IGameMetadata gameDto, CancellationToken cancellationToken,
        IProgress<CopyProgressInfo> copyProgress = null)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (!Directory.Exists(containingFolder) && !File.Exists(containingFolder))
        {
            throw new ArgumentException("The source folder does not exist!", nameof(containingFolder));
        }
        var installFolder =
            Path.Combine(PathUtils.GetGamesFolder(), gameDto.Guid.ToString());
        
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
                var settingsService = Ioc.Default.GetRequiredService<SettingsService>();
                var commandLineToExecute = settingsService.GetUnzipFallbackMethodCommandLine();
                var targetDirectory = PathUtils.GetRandomTempDirectory();
                var commandLineArguments = string.Format(commandLineToExecute.commandLineArguments, containingFolder, targetDirectory);
                var process = Process.Start(commandLineToExecute.command, commandLineArguments);
                await process.WaitForExitAsync();
                return await Install(targetDirectory, gameDto, cancellationToken, copyProgress);
            }
        }

        return installFolder;
    }

    

    private static string MakeValidFileName(string name)
    {
        string invalidChars =
            System.Text.RegularExpressions.Regex.Escape(new string(System.IO.Path.GetInvalidFileNameChars()));
        string invalidRegStr = string.Format(@"([{0}]*\.+$)|([{0}]+)", invalidChars);

        return System.Text.RegularExpressions.Regex.Replace(name, invalidRegStr, "_");
    }

    private static void CopyDirectory(string sourceDir, string destinationDir, bool recursive, IProgress<CopyProgressInfo> progress = null)
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