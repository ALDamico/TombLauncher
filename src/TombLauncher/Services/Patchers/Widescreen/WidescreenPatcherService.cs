using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TombLauncher.Contracts.EngineDetectors;
using TombLauncher.Contracts.Enums;
using TombLauncher.Contracts.Patchers;
using TombLauncher.Core.Extensions;
using TombLauncher.Core.PlatformSpecific;
using TombLauncher.Core.Utils;
using TombLauncher.Data.Database;
using TombLauncher.Data.Models;
using TombLauncher.Localization.Extensions;
using TombLauncher.Patchers.Widescreen;
using TombLauncher.ViewModels;

namespace TombLauncher.Services.Patchers.Widescreen;

public class WidescreenPatcherService
{
    public WidescreenPatcherService(WidescreenPatcher patcher, IEngineDetector engineDetector,
        IDbContextFactory<TombLauncherDbContext> dbContextFactory, IPlatformSpecificFeatures platformSpecificFeatures,
        ILogger<WidescreenPatcherService> logger)
    {
        _patcher = patcher;
        _engineDetector = engineDetector;
        _dbContextFactory = dbContextFactory;
        _platformSpecificFeatures = platformSpecificFeatures;
        _logger = logger;
    }

    private readonly WidescreenPatcher _patcher;
    private readonly IEngineDetector _engineDetector;
    private readonly IDbContextFactory<TombLauncherDbContext> _dbContextFactory;
    private readonly IPlatformSpecificFeatures _platformSpecificFeatures;
    private readonly ILogger<WidescreenPatcherService> _logger;

    public bool Check60FpsSupport(string gameFolder)
    {
        var detectionResult = _engineDetector.Detect(gameFolder);
        return detectionResult.GameEngine is GameEngine.TombRaider2 or GameEngine.TombRaider3;
    }

    public async Task<PatchResult> IsAlreadyApplied(int gameId, CancellationToken cancellationToken)
    {
        await using var ctx = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
        var fileBackup = await RetrieveFileBackup(ctx, gameId, cancellationToken);
        if (fileBackup == null)
        {
            return new PatchResult()
            {
                IsSuccessful = false,
                Message = "PATCH_NOT_YET_APPLIED",
            };
        }

        return new PatchResult()
        {
            IsSuccessful = true,
            AffectedFiles = new List<FileChange>()
            {
                new FileChange()
                {
                    ChangeType = ChangeType.BinaryEdit, Filename = fileBackup.FileName,
                    NewSize = fileBackup.Data?.Length ?? 0, Offset = 0 /* TODO */,
                    OriginalSize = fileBackup.Data?.Length ?? 0
                }
            }
        };
    }

    private static async Task<FileBackup?> RetrieveFileBackup(TombLauncherDbContext ctx, int gameId, CancellationToken cancellationToken)
    {
        return await ctx.FileBackups
            .Where(f => f.GameId == gameId)
            .Where(f => f.FileType == FileType.GameExecutable)
            .Where(f => f.BackupSource == nameof(WidescreenPatcher))
            .OrderByDescending(f => f.BackedUpOn)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<PatchResult> RevertPatch(GameMetadataViewModel gameMetadataViewModel, CancellationToken cancellationToken)
    {
        await using var ctx = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
        var fileBackup = await RetrieveFileBackup(ctx, gameMetadataViewModel.Id, cancellationToken);
        if (fileBackup == null)
        {
            return PatchResult.UnsuccessfulResult("NO_BACKUP_FOUND".GetLocalizedString());
        }

        if (fileBackup.Data.IsNullOrEmpty())
        {
            return PatchResult.UnsuccessfulResult("BACKUP_DATA_EMPTY".GetLocalizedString());
        }

        var targetFiles = Directory.GetFiles(gameMetadataViewModel.InstallDirectory!, fileBackup.FileName,
            _platformSpecificFeatures.GetEnumerationOptions());
        var targetFile = targetFiles.FirstOrDefault();
        if (targetFile.IsNullOrWhiteSpace())
        {
            return PatchResult.UnsuccessfulResult("DESTINATION_FILE_NOT_FOUND".GetLocalizedString(fileBackup.FileName));
        }

        var tempDestination = targetFile + ".tmp";
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

        var md5 = await CryptoUtils.ComputeMd5Hash(tempDestination);
        if (md5 != fileBackup.Md5)
        {
            File.Delete(tempDestination);
            return PatchResult.UnsuccessfulResult("MD5_MISMATCH_IN_TARGET_DATA".GetLocalizedString());
        }

        var fullExecutablePath =
            Path.Combine(gameMetadataViewModel.InstallDirectory!, gameMetadataViewModel.ExecutablePath!);
        
        File.Replace(tempDestination, fullExecutablePath, null);
        return new PatchResult()
        {
            IsSuccessful = true,
            AffectedFiles = new List<FileChange>()
            {
                new FileChange()
                {
                    ChangeType = ChangeType.FileDeletion, Filename = fullExecutablePath,
                    NewSize = fileBackup.Data?.Length ?? 0, OriginalSize = fileBackup.Data?.Length ?? 0
                },
                new FileChange()
                {
                    ChangeType = ChangeType.FileAddition, Filename = fullExecutablePath,
                    NewSize = fileBackup.Data?.Length ?? 0, OriginalSize = fileBackup.Data?.Length ?? 0
                }
            }
        };
    }

    public async Task<PatchResult> ApplyPatch(GameMetadataViewModel viewModel, WidescreenPatcherParameters parameters, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var isAlreadyApplied = await IsAlreadyApplied(viewModel.Id, cancellationToken);

        if (isAlreadyApplied.IsSuccessful)
            return isAlreadyApplied;

        var executablePath = Path.Combine(viewModel.InstallDirectory!, viewModel.ExecutablePath!);

        byte[] binaryData;

        try
        {
            binaryData = await File.ReadAllBytesAsync(executablePath, cancellationToken);
        }
        catch (IOException ex)
        {
            _logger.LogError(ex, "Error while reading file {ExecutablePath}", executablePath);
            return PatchResult.UnsuccessfulResult(
                "ERROR_WHILE_READING_FILE".GetLocalizedString(executablePath, ex.Message));
        }
        var md5 = CryptoUtils.ComputeMd5Hash(binaryData);
        
        var patchResult = await _patcher.ApplyPatch(viewModel.InstallDirectory!, parameters);
        if (!patchResult.IsSuccessful)
            return patchResult;

        var fileBackup = new FileBackup()
        {
            Data = binaryData,
            BackedUpOn = DateTime.Now,
            BackupSource = nameof(WidescreenPatcher),
            FileType = FileType.GameExecutable,
            GameId = viewModel.Id,
            Md5 = md5,
            FileName = viewModel.ExecutablePath!
        };

        await using var dbContext = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
        dbContext.FileBackups.Add(fileBackup);
        await dbContext.SaveChangesAsync(cancellationToken);

        return patchResult;
    }
}