namespace TombLauncher.Core.Dtos;

public class GameSpaceUsedDto
{
    public int Id { get; set; }
    public required string Title { get; set; }
    public long SpaceUsedBytes { get; set; }
}