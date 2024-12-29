using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TombLauncher.Contracts.Enums;
using TombLauncher.Installers.Models;

namespace TombLauncher.Installers;

public class TombRaiderEngineDetector
{
    public TombRaiderEngineDetector()
    {
        _gameEngines = new Dictionary<string, GameEngine>()
        {
            { "tomb3.exe", GameEngine.TombRaider3 },
            { "tomb1main.exe", GameEngine.TombRaider1 },
            { "tomb.exe", GameEngine.TombRaider1 },
            { "tomb2.exe", GameEngine.TombRaider2 },
            { "tomb2main.exe", GameEngine.TombRaider2 },
            { "tomb4.exe", GameEngine.TombRaider4 },
            { "tomb5.exe", GameEngine.TombRaider5 },
            { "tombengine.exe", GameEngine.Ten }
        };
        _knownGameExecutables = _gameEngines.Keys.ToHashSet();
    }

    private Dictionary<string, GameEngine> _gameEngines;
    private HashSet<string> _knownGameExecutables;

    public EngineDetectorResult Detect(string containingFolder)
    {
        var files = Directory.GetFiles(containingFolder, "*.exe", SearchOption.AllDirectories);
        foreach (var file in files)
        {
            if (_gameEngines.TryGetValue(Path.GetFileName(file).ToLowerInvariant(), out var gameEngine))
            {
                var result = new EngineDetectorResult()
                {
                    GameEngine = gameEngine,
                    InstallFolder = containingFolder,
                    ExecutablePath = GetGameExecutablePath(containingFolder),
                    UniversalLauncherPath = GetUniversalLauncherPath(containingFolder)
                };
                return result;
            }
        }

        throw new Exception("Not found :(");
    }

    private string GetUniversalLauncherPath(string containingFolder)
    {
        var universalLauncher = Directory.GetFiles(containingFolder, "play.exe", SearchOption.AllDirectories);
        if (universalLauncher.Length > 0)
            return Path.GetRelativePath(containingFolder, universalLauncher[0]);

        return null;
    }

    private string GetGameExecutablePath(string containingFolder)
    {
        var executables = Directory.GetFiles(containingFolder, "*.exe", SearchOption.AllDirectories);
        var fullPath = _knownGameExecutables.Join(executables, knownExecutable => knownExecutable,
                Path.GetFileName, (s, s1) => s1, StringComparer.InvariantCultureIgnoreCase)
            .FirstOrDefault();

        return Path.GetRelativePath(containingFolder, fullPath);
    }
}