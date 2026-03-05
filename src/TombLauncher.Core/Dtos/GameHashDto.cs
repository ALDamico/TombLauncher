namespace TombLauncher.Core.Dtos;

public class GameHashDto
{
    public int Id { get; set; }
    public int GameId { get; set; }
    public required string FileName { get; set; }
    public required string Md5Hash { get; set; }
}