using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Octokit;
using TombLauncher.Contracts.Downloaders;
using TombLauncher.Contracts.Enums;
using TombLauncher.Contracts.PlatformSpecific;
using TombLauncher.Contracts.Progress;
using TombLauncher.Core.Extensions;
using TombLauncher.Core.Patchers;
using TombLauncher.Core.Utils;
using TombLauncher.Data.Database;
using TombLauncher.Data.Models;
using TombLauncher.Patchers.Tomb4Plus.Enums;
using TombLauncher.Patchers.Tomb4Plus.Models;
using TombLauncher.Patchers.Tomb4Plus.Services;
using FileMode = System.IO.FileMode;

namespace TombLauncher.Services.Patchers.Tomb4Plus;

public class Tomb4PlusPatcherService
{
    private readonly Tomb4PlusFeatureExtractorService _featureExtractorService;
    private readonly IDbContextFactory<TombLauncherDbContext> _dbContextFactory;
    private readonly GitHubClient _gitHubClient;
    private readonly HttpClient _httpClient;
    private readonly IPlatformSpecificFeatures _platformSpecificFeatures;
    private readonly ILogger<Tomb4PlusPatcherService> _logger;

    public Tomb4PlusPatcherService(Tomb4PlusFeatureExtractorService featureExtractorService, 
        IDbContextFactory<TombLauncherDbContext> dbContextFactory, 
        GitHubClient gitHubClient,
        HttpClient httpClient,
        IPlatformSpecificFeatures platformSpecificFeatures,
        ILogger<Tomb4PlusPatcherService> logger)
    {
        _featureExtractorService = featureExtractorService;
        _dbContextFactory = dbContextFactory;
        _gitHubClient = gitHubClient;
        _httpClient = httpClient;
        _platformSpecificFeatures = platformSpecificFeatures;
        _logger = logger;
    }

    public async Task ApplyPatch(IGameMetadataLite gameMetadata, ProgressLogger progressLogger, SyntaxFile syntaxFile, CancellationToken cancellationToken)
    {
        progressLogger.Info("TOMB4PLUS_CONVERSION_STARTED");
        progressLogger.Info("ATTEMPTING_TOMB4PLUS_FEATURE_EXTRACTION");
        try
        {
            var basicGameInfo = new BasicGameMetadata(gameMetadata.Title, gameMetadata.Author!,
                gameMetadata.ReleaseDate.GetValueOrDefault(), Path.GetDirectoryName(gameMetadata.InstallDirectory)!);
            var payload =
                new FeatureExtractorPayload(gameMetadata.InstallDirectory!, gameMetadata.ExecutablePath, basicGameInfo, syntaxFile);
            await _featureExtractorService.Extract(payload, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while extracting info!");
            progressLogger.Error(ex.Message);
            throw;
        }
        
        progressLogger.Info("TOMB4PLUS_FEATURES_EXTRACTION_COMPLETE");
        progressLogger.Info("BACKING_UP_OLD_EXECUTABLE");
        var originalExePath = Path.Combine(gameMetadata.InstallDirectory!, gameMetadata.ExecutablePath!);
        var originalExeBytes = await File.ReadAllBytesAsync(originalExePath, cancellationToken);
        await PerformBackup(originalExePath, originalExeBytes, gameMetadata, cancellationToken);
        progressLogger.Info("OLD_EXECUTABLE_BACKED_UP");
        var downloadPath = PathUtils.GetRandomTempDirectory();
        var extractionPath = Path.Combine(downloadPath, "extracted");
        var fullFilePath = Path.Combine(downloadPath, Path.GetRandomFileName());

        try
        {
            progressLogger.Info("DOWNLOADING_LATEST_TOMB4PLUS_VERSION");
            _logger.LogInformation("Fetching latest Tomb4Plus release from GitHub");
            var releases = await _gitHubClient.Repository.Release.GetAll("SaracenOne", "Tomb4Plus");
            var release = releases?.MaxBy(r => r.CreatedAt);
            if (release == null)
            {
                _logger.LogError("Can't find a release for Tomb4Plus!");
                throw new NotSupportedException("Can't find a release for Tomb4Plus!");
            }

            ReleaseAsset? binaryRelease;
            await using (var file = new FileStream(fullFilePath, FileMode.Create))
            {
                binaryRelease = release.Assets.FirstOrDefault(a => !a.Name.Contains("Source"));
                if (binaryRelease == null)
                {
                    progressLogger.Error("NO_SUITABLE_TOMB4PLUS_RELEASE");
                    throw new NotSupportedException("Can't find a suitable file to download!");
                }
                await _httpClient.DownloadAsync(binaryRelease.BrowserDownloadUrl, file,
                    new Progress<DownloadProgressInfo>(), cancellationToken);
            }
            
            progressLogger.Info("EXTRACTING_TOMB4PLUS_FILES");
            await ExtractZipContent(fullFilePath, extractionPath, cancellationToken);
        }
        catch (Exception ex)
        {
            progressLogger.Error("ERROR_WHILE_PATCHING");
            _logger.LogError(ex, "Error while applying Tomb4Plus patch!");
            throw;
        }

        var extractedFiles =
            Directory.EnumerateFiles(extractionPath, "*", _platformSpecificFeatures.GetEnumerationOptions())
                .ToList();

        var exeFile = extractedFiles.FirstOrDefault(f => f.EndsWith(".exe"));
        if (exeFile.IsNullOrWhiteSpace())
            throw new FileNotFoundException("Executable file not found among extracted files!");
        var exeBytes = await File.ReadAllBytesAsync(exeFile, cancellationToken);
        var targetExePath = Path.Combine(gameMetadata.InstallDirectory!, Path.GetFileName(exeFile));
        progressLogger.Info("SAVING_NEW_EXECUTABLE");
        await PerformBackup(targetExePath, exeBytes, gameMetadata, cancellationToken);

        try
        {
            foreach (var file in extractedFiles)
            {
                var relativePath = Path.GetRelativePath(downloadPath, file);
                var targetFilePath = Path.Combine(gameMetadata.InstallDirectory!, relativePath);
                progressLogger.Info("MOVING_FILE", file);
                File.Move(file, targetFilePath, true);
            }
            
            progressLogger.Info("REMOVING_OLD_FILE", originalExePath);
            File.Delete(originalExePath);
        }
        finally
        {
            if (Directory.Exists(downloadPath))
                Directory.Delete(downloadPath, true);
        }
    }

    private async Task ExtractZipContent(string fullFilePath, string destinationPath, CancellationToken cancellationToken)
    {
        using var zipManager = new ZipManager(fullFilePath);
        var entries = zipManager.GetEntries()
            .Where(e => e?.IsDirectory == false)
            .Where(e => e != null)
            .Where(e => e!.Name.StartsWith("x86/"));

        foreach (var entry in entries)
        {
            if (entry == null)
                continue;
            await using var inputStream = zipManager.GetInputStream(entry);
            var relativePath = entry.Name["x86/".Length..]; // strips "x86/" prefix
            var fullDestinationPath = Path.Combine(destinationPath, relativePath);
            Directory.CreateDirectory(Path.GetDirectoryName(fullDestinationPath)!);
            await using var destFile = File.Create(fullDestinationPath);
            await inputStream.CopyToAsync(destFile, cancellationToken);
        }
    }

    private async Task PerformBackup(string path, byte[] bytes, IGameMetadataLite gameMetadata, CancellationToken cancellationToken)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
        
        var md5 = CryptoUtils.ComputeMd5Hash(bytes);

        var fileBackup = new FileBackup()
        {
            Data = bytes,
            BackedUpOn = DateTime.Now,
            BackupSource = nameof(Tomb4PlusFeatureExtractorService),
            FileType = FileType.GameExecutable,
            GameId = gameMetadata.Id,
            Md5 = md5,
            FileName = path
        };

        dbContext.FileBackups.Add(fileBackup);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}