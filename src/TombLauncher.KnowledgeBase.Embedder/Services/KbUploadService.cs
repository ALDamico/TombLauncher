using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Renci.SshNet;
using TombLauncher.Ai.Configuration;
using TombLauncher.Ai.Models;
using TombLauncher.Core.Utils;
using TombLauncher.KnowledgeBase.Embedder.Configuration;

namespace TombLauncher.KnowledgeBase.Embedder.Services;

public class KbUploadService
{
    private readonly SftpConfig _sftp;
    private readonly string _kbPath;
    private readonly ILogger<KbUploadService> _logger;

    public const int KbSchemaVersion = 1;

    public KbUploadService(IOptions<SftpConfig> sftpOptions, IOptions<AiConfig> aiOptions, ILogger<KbUploadService> logger)
    {
        _sftp = sftpOptions.Value;
        _kbPath = aiOptions.Value.KnowledgeBasePath ?? "./kb_embeddings.db";
        _logger = logger;
    }

    public async Task UploadAsync(CancellationToken cancellationToken)
    {
        if (!File.Exists(_kbPath))
        {
            _logger.LogError("KB file not found at {Path}", _kbPath);
            return;
        }

        _logger.LogInformation("Computing SHA-256 of {Path}", _kbPath);
        await using var hashStream = File.OpenRead(_kbPath);
        var sha256 = await CryptoUtils.ComputeSha256Hash(hashStream, cancellationToken);

        var manifest = new KbManifest
        {
            GeneratedAt = DateTimeOffset.UtcNow,
            SchemaVersion = KbSchemaVersion,
            Sha256 = sha256
        };

        var manifestJson = JsonSerializer.Serialize(manifest, new JsonSerializerOptions { WriteIndented = true });
        var remoteTmp = RemotePath("kb_embeddings.db.tmp");
        var remoteFinal = RemotePath("kb_embeddings.db");
        var remoteManifest = RemotePath("kb_manifest.json");

        _logger.LogInformation("Connecting to {Host}:{Port}", _sftp.Host, _sftp.Port);
        using var client = new SftpClient(BuildConnectionInfo());
        client.Connect();

        _logger.LogInformation("Uploading KB to {Remote}", remoteTmp);
        await using (var stream = File.OpenRead(_kbPath))
        {
            await Task.Run(() => client.UploadFile(stream, remoteTmp, canOverride: true), cancellationToken);
        }

        _logger.LogInformation("Renaming {Tmp} → {Final}", remoteTmp, remoteFinal);
        client.RenameFile(remoteTmp, remoteFinal);

        _logger.LogInformation("Uploading manifest to {Remote}", remoteManifest);
        await using (var ms = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(manifestJson)))
        {
            await Task.Run(() => client.UploadFile(ms, remoteManifest, canOverride: true), cancellationToken);
        }

        _logger.LogInformation("Upload complete — SHA-256: {Hash}", sha256);
        client.Disconnect();
    }

    private ConnectionInfo BuildConnectionInfo()
    {
        AuthenticationMethod auth = !string.IsNullOrEmpty(_sftp.PrivateKeyPath)
            ? new PrivateKeyAuthenticationMethod(_sftp.Username,
                string.IsNullOrEmpty(_sftp.PrivateKeyPassphrase)
                    ? new PrivateKeyFile(_sftp.PrivateKeyPath)
                    : new PrivateKeyFile(_sftp.PrivateKeyPath, _sftp.PrivateKeyPassphrase))
            : new PasswordAuthenticationMethod(_sftp.Username, _sftp.Password);

        return new ConnectionInfo(_sftp.Host, _sftp.Port, _sftp.Username, auth);
    }

    private string RemotePath(string fileName) =>
        _sftp.RemotePath.TrimEnd('/') + "/" + fileName;
}
