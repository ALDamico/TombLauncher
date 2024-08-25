namespace TombLauncher.Dto;

public class GameHashDto
{
    public int Id { get; set; }
    public int GameId { get; set; }
    public string FileName { get; set; }
    public string Md5Hash { get; set; }
}