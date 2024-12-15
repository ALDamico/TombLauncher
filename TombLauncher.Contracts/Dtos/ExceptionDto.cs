namespace TombLauncher.Contracts.Dtos;

public class ExceptionDto
{
    public string Type { get; set; }
    public string Message { get; set; }
    public string HelpLink { get; set; }
    public string StackTrace { get; set; }
    public string Source { get; set; }
    public ExceptionDto InnerException { get; set; }
}