using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TombLauncher.Contracts.EngineDetectors;
using TombLauncher.Contracts.Enums;
using TombLauncher.Contracts.PlatformSpecific;
using TombLauncher.Core.Extensions;
using TombLauncher.Core.PlatformSpecific;
using TombLauncher.Core.Utils;

namespace TombLauncher.Installers;

public class TombRaiderEngineDetector : IEngineDetector
{
    private readonly IPlatformSpecificFeatures _platformSpecificFeatures;

    public TombRaiderEngineDetector(IPlatformSpecificFeatures platformSpecificFeatures)
    {
        _platformSpecificFeatures = platformSpecificFeatures;
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
            { "tr2x.exe", GameEngine.Tr2x },
            { "trx.exe", GameEngine.Trx }
        };

        if (_platformSpecificFeatures.Platform == Platform.Linux)
        {
            // These can occur if the native patch has been applied
            _gameEngines.Add("tr1x", GameEngine.Tr1x);
            _gameEngines.Add("tr2x", GameEngine.Tr2x);
            _gameEngines.Add("trx", GameEngine.Trx);
        }
        _knownGameExecutables = _gameEngines.Keys.ToHashSet();
    }

    private readonly Dictionary<string, GameEngine> _gameEngines;
    private readonly HashSet<string> _knownGameExecutables;

    public EngineDetectorResult Detect(string containingFolder)
    {
        var files = Directory.GetFiles(containingFolder, "*.exe", _platformSpecificFeatures.GetEnumerationOptions());
        var result = new EngineDetectorResult()
        {
            GameEngine = GameEngine.Unknown,
            InstallFolder = containingFolder,
            ExecutablePath = GetGameExecutablePath(containingFolder),
            UniversalLauncherPath = GetUniversalLauncherPath(containingFolder)
        };
        ProcessFiles(containingFolder, files, result);

        if (files.Length != 0 || result.GameEngine != GameEngine.Unknown) 
            return result;
        var allFiles = Directory.GetFiles(containingFolder, "*", _platformSpecificFeatures.GetEnumerationOptions())
            .Where(f => _gameEngines.Keys.Any(k => f.EndsWith(k, StringComparison.InvariantCultureIgnoreCase))).ToArray();
        ProcessFiles(containingFolder, allFiles, result);

        return result;
    }

    private void ProcessFiles(string containingFolder, string[] files, EngineDetectorResult result)
    {
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
                        if (PathUtils.DirectoryContainsFile(containingFolder, "tomb3.dll") || PathUtils.DirectoryContainsFile(containingFolder, "tomb3main.dll"))
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
    }

    private string? GetUniversalLauncherPath(string containingFolder)
    {
        var universalLauncher = Directory.GetFiles(containingFolder, "play.exe", _platformSpecificFeatures.GetEnumerationOptions());
        if (universalLauncher.Length > 0)
            return Path.GetRelativePath(containingFolder, universalLauncher[0]);

        return null;
    }

    private string? GetGameExecutablePath(string containingFolder)
    {
        var executables = Directory.GetFiles(containingFolder, "*", _platformSpecificFeatures.GetEnumerationOptions());
        var fullPath = _knownGameExecutables.Join(executables, knownExecutable => knownExecutable,
                Path.GetFileName, (_, s1) => s1, StringComparer.InvariantCultureIgnoreCase)
            .FirstOrDefault();

        if (fullPath.IsNullOrWhiteSpace())
            return null;
        
        return Path.GetRelativePath(containingFolder, fullPath ?? string.Empty);
    }
}