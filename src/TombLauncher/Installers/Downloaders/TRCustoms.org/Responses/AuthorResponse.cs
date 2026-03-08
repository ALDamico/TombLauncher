namespace TombLauncher.Installers.Downloaders.TRCustoms.org.Responses;

public class AuthorResponse
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public FileResponse Picture { get; set; } = null!;
}