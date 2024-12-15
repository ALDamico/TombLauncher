namespace TombLauncher.Data.Models;

public class ApplicationSetting
{
    public int Id { get; set; }
    public string SettingName { get; set; }
    public string StringValue { get; set; }
    public int? IntValue { get; set; }
    public double? DoubleValue { get; set; }
    public bool? BoolValue { get; set; }
    public DateTime? DateTimeValue { get; set; }
}