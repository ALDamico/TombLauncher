using System.IO.Compression;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using TombLauncher.Ai.Configuration;
using TombLauncher.Ai.Models;
using TombLauncher.Core.Extensions;
using TombLauncher.Core.PlatformSpecific;
using TombLauncher.Core.Utils;
using TombLauncher.Localization.Extensions;

namespace TombLauncher.Ai.Services;

public class KbUpdateService
{
    private const string RemoteManifestUrl = "https://tomblauncher.app/kb/kb_manifest.json";
    private const string RemoteDbUrl = "https://tomblauncher.app/kb/kb_embeddings.zip";

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly string _localDbPath;
    private readonly string _localManifestPath;
    private readonly ILogger<KbUpdateService> _logger;

    public KbUpdateService(IHttpClientFactory httpClientFactory, IAiConfig config,
        IPlatformSpecificFeatures platformSpecificFeatures, ILogger<KbUpdateService> logger)
    {
        var appDataDir = platformSpecificFeatures.GetAppDataDirectory();
        _localDbPath = Path.Combine(appDataDir, config.KnowledgeBasePath ?? "KnowledgeBase/kb_embeddings.db");
        _localManifestPath = Path.Combine(Path.GetDirectoryName(_localDbPath)!, "kb_manifest.json");
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task<KbUpdateResult> CheckAndUpdateAsync(IProgress<string>? progress, CancellationToken ct)
    {
        _logger.LogInformation("Checking for KB update");

        KbManifest remoteManifest;
        try
        {
            using var client = _httpClientFactory.CreateClient();
            client.Timeout = TimeSpan.FromSeconds(3);
            var json = await client.GetStringAsync(RemoteManifestUrl, ct);
            remoteManifest = JsonSerializer.Deserialize<KbManifest>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            _logger.LogWarning(ex, "Failed to fetch remote KB manifest");
            return KbUpdateResult.Error("MANIFEST_FETCH_FAILURE", ex);
        }

        if (remoteManifest.SchemaVersion > KbManifest.SupportedSchemaVersion)
        {
            _logger.LogWarning("Remote KB schema {Remote} > supported {Supported} — skipping",
                remoteManifest.SchemaVersion, KbManifest.SupportedSchemaVersion);
            progress?.Report("KB_SCHEMA_TOO_NEW");
            return KbUpdateResult.Success();
        }

        var local = ReadLocalManifest();
        if (local != null && remoteManifest.GeneratedAt <= local.GeneratedAt)
        {
            _logger.LogInformation("KB is already up to date ({Date})", local.GeneratedAt);
            progress?.Report("KB_UP_TO_DATE".GetLocalizedString());
            return KbUpdateResult.Success();
        }

        progress?.Report("KB_DOWNLOADING".GetLocalizedString());
        Directory.CreateDirectory(Path.GetDirectoryName(_localDbPath)!);
        var tmpZipPath = _localDbPath + ".zip.tmp";
        var tmpDbPath = _localDbPath + ".tmp";

        try
        {
            using var client = _httpClientFactory.CreateClient();
            await using var fileStream = File.Create(tmpZipPath);
            await client.DownloadAsync(RemoteDbUrl, fileStream, cancellationToken: ct);
        }
        catch(Exception ex)
        {
            File.Delete(tmpZipPath);
            _logger.LogError(ex, "KB zip file download failed");
            return KbUpdateResult.Error("KB_DOWNLOAD_FAILURE", ex);
        }

        try
        {
            await using var zip = await ZipFile.OpenReadAsync(tmpZipPath, ct);
            var entry = zip.Entries.FirstOrDefault(e => e.Name.EndsWith(".db", StringComparison.OrdinalIgnoreCase));
            if (entry == null)
            {
                _logger.LogError("No .db entry found in downloaded zip");
                progress?.Report("KB_INTEGRITY_ERROR");
                return KbUpdateResult.Error("KB_INTEGRITY_ERROR");
            }
            await entry.ExtractToFileAsync(tmpDbPath, overwrite: true, cancellationToken:ct);
        }
        finally
        {
            File.Delete(tmpZipPath);
        }

        progress?.Report("KB_VERIFYING");
        await using (var hashStream = File.OpenRead(tmpDbPath))
        {
            var actual = await CryptoUtils.ComputeSha256Hash(hashStream, ct);
            if (!string.Equals(actual, remoteManifest.Sha256, StringComparison.OrdinalIgnoreCase))
            {
                File.Delete(tmpDbPath);
                _logger.LogError("SHA-256 mismatch: expected {Expected}, got {Actual}", remoteManifest.Sha256, actual);
                progress?.Report("KB_INTEGRITY_ERROR");
                return KbUpdateResult.Error("KB_INTEGRITY_ERROR");
            }
        }

        File.Move(tmpDbPath, _localDbPath, overwrite: true);
        await File.WriteAllTextAsync(_localManifestPath,
            JsonSerializer.Serialize(remoteManifest), ct);

        _logger.LogInformation("KB updated to {Date}", remoteManifest.GeneratedAt);
        progress?.Report("KB_UPDATED");
        return KbUpdateResult.Success();
    }

    private KbManifest? ReadLocalManifest()
    {
        if (!File.Exists(_localManifestPath)) return null;
        try
        {
            var json = File.ReadAllText(_localManifestPath);
            return JsonSerializer.Deserialize<KbManifest>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }
        catch
        {
            return null;
        }
    }
}
