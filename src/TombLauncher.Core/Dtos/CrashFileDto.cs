namespace TombLauncher.Core.Dtos;

public class CrashFileDto
{
    public CrashFileDto(string fileName, string content)
    {
        FileName = fileName;
        Content = content;
    }

    public string FileName { get; set; }
    public string Content { get; set; }
}