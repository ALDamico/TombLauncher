using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Octokit;
using TombLauncher.Configuration;
using TombLauncher.Configuration.Sections;
using TombLauncher.Contracts.Downloaders;
using TombLauncher.Contracts.Enums;
using TombLauncher.Contracts.Patchers;
using TombLauncher.Contracts.PlatformSpecific;
using TombLauncher.Contracts.Progress;
using TombLauncher.Core.Extensions;
using TombLauncher.Core.Patchers;
using TombLauncher.Core.PlatformSpecific;
using TombLauncher.Core.Utils;
using TombLauncher.Data.Database;
using TombLauncher.Data.Models;
using TombLauncher.Localization.Extensions;
using TombLauncher.Patchers.Trx.Patchers;
using ChangeType = TombLauncher.Contracts.Patchers.ChangeType;
using FileMode = System.IO.FileMode;

namespace TombLauncher.Services.Patchers.TrxNative;

public class TrxNativePatcherService : IViewService
{
    private readonly IDbContextFactory<TombLauncherDbContext> _dbContextFactory;
    private readonly TrxNativeExecutablePatcher _patcher;
    private readonly IPlatformSpecificFeatures _platformSpecificFeatures;
    private readonly GitHubClient _gitHubClient;
    private readonly HttpClient _httpClient;
    private readonly ILogger<TrxNativePatcherService> _logger;
    private readonly ICompatibilityConfig _compatibilityConfig;

    public TrxNativePatcherService(IDbContextFactory<TombLauncherDbContext> dbContextFactory, 
        TrxNativeExecutablePatcher patcher,
        IPlatformSpecificFeatures platformSpecificFeatures, 
        GitHubClient gitHubClient,
        HttpClient httpClient,
        ViewServiceContext viewContext,
        ILayeredAppConfiguration appConfiguration,
        ILogger<TrxNativePatcherService> logger)
    {
        _dbContextFactory = dbContextFactory;
        _patcher = patcher;
        _platformSpecificFeatures = platformSpecificFeatures;
        _gitHubClient = gitHubClient;
        _httpClient = httpClient;
        _logger = logger;
        ViewContext = viewContext;
        _compatibilityConfig = appConfiguration.Compatibility;
    }

    public TrxVersionInfo GetVersionInfo(string executablePath, IProgress<string> progress)
    {
        progress.Report("DETECTING_VERSION_INFO".GetLocalizedString());
        return VersionUtils.ReadTrxVersionInfo(executablePath);
    }

    public async Task<bool> IsAlreadyApplied(int gameId, ProgressLogger progress, CancellationToken ct)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync(ct);
        var fileBackup = await RetrieveFileBackup(dbContext, gameId, progress, ct);
        return fileBackup != null;
    }
    
    private static async Task<FileBackup?> RetrieveFileBackup(TombLauncherDbContext ctx, int gameId, ProgressLogger progress, CancellationToken cancellationToken)
    {
        progress.Info("RETRIEVING_EXISTING_BACKUP");
        return await ctx.FileBackups
            .Where(f => f.GameId == gameId)
            .Where(f => f.FileType == FileType.GameExecutable)
            .Where(f => f.BackupSource == nameof(TrxNativeExecutablePatcher))
            .OrderBy(f => f.BackedUpOn)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<PatchResult> ApplyPatch(IGameMetadataLite vm, ProgressLogger progress,
        CancellationToken ct)
    {
        var downloadPath = PathUtils.GetRandomTempDirectory();
        var originalExePath = Path.Combine(vm.InstallDirectory!, vm.ExecutablePath!);
        var targetExePath = Regex.Replace(originalExePath, @"\.exe$", "");
        var tempExePath = "";
        var fullFilePath = Path.Combine(downloadPath, Path.GetRandomFileName());
        try
        {
            progress.Info("READING_EXECUTABLE_VERSION");
            var versionInfo = VersionUtils.ReadTrxVersionInfo(originalExePath);

            progress.Info("DOWNLOADING_NATIVE_EXECUTABLE_FROM_GITHUB");
            var tag = _patcher.BuildTag(versionInfo.InternalName, versionInfo.Version);
            var release = await _gitHubClient.Repository.Release.Get("LostArtefacts", "TRX", tag);

            var nativeRelease = release.Assets
                .Where(a => a.Name.Contains(_platformSpecificFeatures.Platform.ToString(),
                    StringComparison.InvariantCultureIgnoreCase))
                .FirstOrDefault(a => a.Name.EndsWith("zip"));

            if (nativeRelease == null)
                throw new NotSupportedException(
                    $"Can't find a release for platform {_platformSpecificFeatures.Platform} for {versionInfo.InternalName} with version {versionInfo.Version}");

            await using var file = new FileStream(fullFilePath, FileMode.Create);
            await _httpClient.DownloadAsync(nativeRelease.BrowserDownloadUrl, file, new Progress<DownloadProgressInfo>(), ct);

            var exeBytes = await _patcher.ExtractExecutable(fullFilePath, ct);

            if (exeBytes.Length == 0)
                return PatchResult.UnsuccessfulResult("UNABLE_TO_EXTRACT_EXECUTABLE".GetLocalizedString());

            tempExePath = Path.Combine(downloadPath, versionInfo.ExecutableName);

            try
            {
                await File.WriteAllBytesAsync(tempExePath, exeBytes, ct);
            }
            catch (IOException ex)
            {
                _logger.LogError(ex, "An error occurred while extracting the TRX executable");
                return PatchResult.UnsuccessfulResult($"Error while applying patch: {ex.Message}");
            }

            var originalExeBytes = await File.ReadAllBytesAsync(originalExePath, ct);
            var md5 = CryptoUtils.ComputeMd5Hash(originalExeBytes);

            var fileBackup = new FileBackup()
            {
                Data = originalExeBytes,
                BackedUpOn = DateTime.Now,
                BackupSource = nameof(TrxNativeExecutablePatcher),
                FileType = FileType.GameExecutable,
                GameId = vm.Id,
                Md5 = md5,
                FileName = vm.ExecutablePath!
            };

            var newExecutableBackup = new FileBackup()
            {
                Data = exeBytes,
                BackedUpOn = DateTime.Now.AddSeconds(1),
                BackupSource = nameof(TrxNativeExecutablePatcher),
                FileName = Path.GetFileNameWithoutExtension(vm.ExecutablePath!),
                FileType = FileType.GameExecutable,
                Md5 = CryptoUtils.ComputeMd5Hash(exeBytes),
                GameId = vm.Id,
            };
            await using var dbContext = await _dbContextFactory.CreateDbContextAsync(ct);
            dbContext.FileBackups.Add(fileBackup);
            dbContext.FileBackups.Add(newExecutableBackup);
            var game = await dbContext.Games.FindAsync([vm.Id], ct);
            game?.CompatibilityTool = CompatibilityTool.LinuxNative;
            await dbContext.SaveChangesAsync(ct);

            File.Move(tempExePath, targetExePath, overwrite: true);
            File.SetUnixFileMode(targetExePath,
                UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute |
                UnixFileMode.GroupRead | UnixFileMode.GroupExecute |
                UnixFileMode.OtherRead | UnixFileMode.OtherExecute);
            File.Delete(originalExePath);

            return new PatchResult()
            {
                IsSuccessful = true,
                AffectedFiles =
                [
                    new FileChange()
                    {
                        ChangeType = ChangeType.FileAddition, Filename = versionInfo.ExecutableName,
                        NewSize = exeBytes.Length, OriginalSize = 0
                    },
                    new FileChange()
                    {
                        ChangeType = ChangeType.FileDeletion, Filename = vm.ExecutablePath!, NewSize = 0,
                        OriginalSize = originalExeBytes.Length
                    }
                ]
            };
        }
        finally
        {
            if (tempExePath.Length > 0 && File.Exists(tempExePath))
                File.Delete(tempExePath);
            if (File.Exists(fullFilePath))
                File.Delete(fullFilePath);
        }
    }

    public async Task<PatchResult> RevertPatch(IGameMetadataLite gameMetadataViewModel, ProgressLogger progress, CancellationToken cancellationToken)
    {
        await using var ctx = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
        var fileBackup = await RetrieveFileBackup(ctx, gameMetadataViewModel.Id, progress, cancellationToken);
        if (fileBackup == null)
        {
            return PatchResult.UnsuccessfulResult("NO_BACKUP_FOUND".GetLocalizedString());
        }

        if (fileBackup.Data.IsNullOrEmpty())
        {
            return PatchResult.UnsuccessfulResult("BACKUP_DATA_EMPTY".GetLocalizedString());
        }

        var tempFilePath = PathUtils.GetRandomTempDirectory();
        var tempDestination = Path.Combine(tempFilePath, fileBackup.FileName);
        var tempDestinationFolder = Path.GetDirectoryName(tempDestination);
        Directory.CreateDirectory(tempDestinationFolder!);
        try
        {
            await File.WriteAllBytesAsync(tempDestination, fileBackup.Data!, cancellationToken);
        }
        catch (IOException ex)
        {
            _logger.LogError(ex, "Error while restoring backup");
            return PatchResult.UnsuccessfulResult(
                "ERROR_WHILE_RESTORING_BACKUP".GetLocalizedString(fileBackup.FileName, ex.Message));
        }

        progress.Info("CHECKING_GAME_EXECUTABLE_CONSISTENCY");
        var md5 = await CryptoUtils.ComputeMd5Hash(tempDestination);
        if (md5 != fileBackup.Md5)
        {
            File.Delete(tempDestination);
            return PatchResult.UnsuccessfulResult("MD5_MISMATCH_IN_TARGET_DATA".GetLocalizedString());
        }

        progress.Info("RESTORING_GAME_EXECUTABLE");
        var fullExecutablePath = Path.Combine(gameMetadataViewModel.InstallDirectory!, fileBackup.FileName);
        var nativeBinaryPath = Path.Combine(gameMetadataViewModel.InstallDirectory!,
            Path.GetFileNameWithoutExtension(fileBackup.FileName));

        File.Move(tempDestination, fullExecutablePath, overwrite: true);
        if (File.Exists(nativeBinaryPath))
            File.Delete(nativeBinaryPath);

        gameMetadataViewModel.ExecutablePath = fileBackup.FileName;
        var allPatcherBackups = await ctx.FileBackups
            .Where(f => f.GameId == gameMetadataViewModel.Id)
            .Where(f => f.FileType == FileType.GameExecutable)
            .Where(f => f.BackupSource == nameof(TrxNativeExecutablePatcher))
            .ToListAsync(cancellationToken);
        ctx.FileBackups.RemoveRange(allPatcherBackups);
        var game = await ctx.Games.FindAsync([gameMetadataViewModel.Id], cancellationToken);
        game?.CompatibilityTool = _compatibilityConfig.CompatibilityTool;
        await ctx.SaveChangesAsync(cancellationToken);
        return new PatchResult()
        {
            IsSuccessful = true,
            AffectedFiles =
            [
                new FileChange()
                {
                    ChangeType = ChangeType.FileDeletion, Filename = nativeBinaryPath,
                    NewSize = 0, OriginalSize = fileBackup.Data?.Length ?? 0
                },

                new FileChange()
                {
                    ChangeType = ChangeType.FileAddition, Filename = fullExecutablePath,
                    NewSize = fileBackup.Data?.Length ?? 0, OriginalSize = 0
                }
            ]
        };
    }

    public ViewServiceContext ViewContext { get; }
}