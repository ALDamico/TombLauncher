namespace TombLauncher.KnowledgeBase.Embedder.Configuration;

public class SftpConfig
{
    public string Host { get; set; } = "";
    public int Port { get; set; } = 22;
    public string Username { get; set; } = "";
    public string? Password { get; set; }
    public string? PrivateKeyPath { get; set; }
    public string? PrivateKeyPassphrase { get; set; }
    public string RemotePath { get; set; } = "";
}
