using TombLauncher.Contracts.Enums;

namespace TombLauncher.Data.Models;

public class GameDocs
{
    public int Id { get; set; }
    public int GameId { get; set; }
    public string Path { get; set; }
    public DocType DocType { get; set; }
}