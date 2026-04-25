namespace TombLauncher.Core.Dtos;

public class AppCrashDto
{
    public int Id { get; set; }
    public required ExceptionDto ExceptionDto { get; set; }
    public DateTime DateTime { get; set; }
}