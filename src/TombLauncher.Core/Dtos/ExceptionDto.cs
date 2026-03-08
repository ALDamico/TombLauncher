namespace TombLauncher.Core.Dtos;

public class ExceptionDto
{
    public required string Type { get; set; }
    public required string Message { get; set; }
    public string? HelpLink { get; set; }
    public required string StackTrace { get; set; }
    public required string Source { get; set; }
    public ExceptionDto? InnerException { get; set; }
}