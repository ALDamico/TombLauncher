namespace TombLauncher.Data.Models;

public class GameHashes
{
    public int Id { get; set; }
    public int GameId { get; set; }
    public required string Md5Hash { get; set; }
    public required string FileName { get; set; }
}