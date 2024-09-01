namespace TombLauncher.Models;

public class GameLink
{
    public int Id { get; set; }
    public int GameId { get; set; }
    public LinkType LinkType { get; set; }
    public string Link { get; set; }
}