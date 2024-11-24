namespace TombLauncher.Data.Dto;

public class PlaySessionDto
{
    public int Id { get; set; }
    public int GameId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}