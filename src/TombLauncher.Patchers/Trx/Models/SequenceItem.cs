namespace TombLauncher.Patchers.Trx.Models;

public class SequenceItem
{
    public required string Type { get; set; }
    public string? Path { get; set; }
    public bool? Legal { get; set; }
    public int? DisplayTime { get; set; }
    public double? FadeInTime { get; set; }
    public double? FadeOutTime { get; set; }
}