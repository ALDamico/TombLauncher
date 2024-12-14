namespace TombLauncher.Contracts.Settings;

public interface ISettingsVisitable
{
    void Accept(ISettingsVisitor visitor);
}