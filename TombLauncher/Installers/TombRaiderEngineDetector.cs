using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TombLauncher.Contracts.Enums;
using TombLauncher.Core.Extensions;
using TombLauncher.Core.Utils;
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

                if (gameEngine.HasFlag(GameEngine.TombRaider2) || gameEngine.HasFlag(GameEngine.TombRaider3) ||
                    gameEngine.HasFlag(GameEngine.TombRaider4) || gameEngine.HasFlag(GameEngine.TombRaider5))
                {
                    result.SetupExecutablePath = result.ExecutablePath;
                    result.SetupArgs = "-setup";
                }
                
                switch (gameEngine)
                {
                    case GameEngine.TombRaider2:
                    {
                        if (PathUtils.DirectoryContainsFile(containingFolder, "TR2Main.dll"))
                        {
                            result.GameEngine = GameEngine.Tomb2Main;
                        }

                        break;
                    }
                    case GameEngine.TombRaider3:
                    {
                        if (PathUtils.DirectoryContainsFile(containingFolder, "tomb3.dll"))
                        {
                            result.GameEngine = GameEngine.Tomb3CommunityEdition;

                            if (PathUtils.DirectoryContainsFile(containingFolder, "tomb3_ConfigTool.exe"))
                            {
                                result.CommunitySetupExecutablePath =
                                    PathUtils.GetRelativePath(containingFolder, "tomb3_ConfigTool.exe");
                            }
                        }

                        break;
                    }
                    case GameEngine.Tr1x:
                    {
                        if (PathUtils.DirectoryContainsFile(containingFolder, "TR1X_ConfigTool.exe"))
                        {
                            result.CommunitySetupExecutablePath =
                                PathUtils.GetRelativePath(containingFolder, "TR1X_ConfigTool.exe");
                        }

                        break;
                    }
                    case GameEngine.Tr2x:
                    {
                        if (PathUtils.DirectoryContainsFile(containingFolder, "TR2X_ConfigTool.exe"))
                        {
                            result.CommunitySetupExecutablePath =
                                PathUtils.GetRelativePath(containingFolder, "TR2X_ConfigTool.exe");
                        }

                        break;
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