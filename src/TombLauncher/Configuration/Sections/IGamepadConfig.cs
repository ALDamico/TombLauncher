using System.Collections.Generic;

namespace TombLauncher.Configuration.Sections;

public interface IGamepadConfig
{
    string? AntiMicroXPath { get; }
    Dictionary<string, string?>? Profiles { get; }
}