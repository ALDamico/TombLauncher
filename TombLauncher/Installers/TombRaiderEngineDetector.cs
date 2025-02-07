using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TombLauncher.Contracts.Enums;
using TombLauncher.Core.Extensions;
using TombLauncher.Installers.Models;

namespace TombLauncher.Installers;

public class TombRaiderEngineDetector
{
    public TombRaiderEngineDetector()
    {
        _gameEngines = new Dictionary<string, GameEngine>()
        {
            { "tomb3.exe", GameEngine.TombRaider3 },
            { "tombati.exe", GameEngine.TombAti },
            { "tomb.exe", GameEngine.TombRaider1 },
            { "tomb2.exe", GameEngine.TombRaider2 },
            { "tomb2main.exe", GameEngine.TombRaider2 },
            { "tomb4.exe", GameEngine.TombRaider4 },
            { "tomb5.exe", GameEngine.TombRaider5 },
            { "tombengine.exe", GameEngine.Ten },
            { "tomb1main.exe", GameEngine.Tr1x },
            { "tr1x.exe", GameEngine.Tr1x },
            { "tr2x.exe", GameEngine.Tr2x }
        };
        _knownGameExecutables = _gameEngines.Keys.ToHashSet();
    }

    private Dictionary<string, GameEngine> _gameEngines;
    private HashSet<string> _knownGameExecutables;

    public EngineDetectorResult Detect(string containingFolder)
    {
        var files = Directory.GetFiles(containingFolder, "*.exe", SearchOption.AllDirectories);
        var result = new EngineDetectorResult()
        {
            GameEngine = GameEngine.Unknown,
            InstallFolder = containingFolder,
            ExecutablePath = GetGameExecutablePath(containingFolder),
            UniversalLauncherPath = GetUniversalLauncherPath(containingFolder)
        };
        foreach (var file in files)
        {
            if (_gameEngines.TryGetValue(Path.GetFileName(file).ToLowerInvariant(), out var gameEngine))
            {
                result.GameEngine = gameEngine;
                if (gameEngine == GameEngine.TombRaider2)
                {
                    var hasTomb2MainFiles = Directory.GetFiles(containingFolder, "TR2Main.dll");
                    if (hasTomb2MainFiles?.Length > 0)
                    {
                        result.GameEngine = GameEngine.Tomb2Main;
                    }
                }
            }
        }

        return result;
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

        if (fullPath.IsNullOrWhiteSpace())
            return null;

        return Path.GetRelativePath(containingFolder, fullPath);
    }
}