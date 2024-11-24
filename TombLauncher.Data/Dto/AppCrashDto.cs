namespace TombLauncher.Data.Dto;

public class AppCrashDto
{
    public int Id { get; set; }
    public ExceptionDto ExceptionDto { get; set; }
    public DateTime DateTime { get; set; }
}