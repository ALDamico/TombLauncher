namespace TombLauncher.Data.Models;

public class GameHashes
{
    public int Id { get; set; }
    public int GameId { get; set; }
    public string Md5Hash { get; set; }
    public string FileName { get; set; }
}