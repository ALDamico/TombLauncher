namespace TombLauncher.Data.Models;

public class AppCrash
{
    public int Id { get; set; }
    // ReSharper disable once EntityFramework.ModelValidation.UnlimitedStringLength
    public required string Exception { get; set; }
    public DateTime DateTime { get; set; }
    public bool WasNotified { get; set; }
}