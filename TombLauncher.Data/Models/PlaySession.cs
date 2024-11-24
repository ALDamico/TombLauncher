namespace TombLauncher.Data.Models;

public class PlaySession
{
    public int Id { get; set; }
    public int GameId { get; set; }
    public Game Game { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}