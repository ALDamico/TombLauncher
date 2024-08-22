using System;
using System.Collections.Generic;
using System.IO;
using TombLauncher.Models.Models;

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
            { "tomb5.exe", GameEngine.TombRaider5 }
        };
    }
    private Dictionary<string, GameEngine> _gameEngines;
    public GameEngine Detect(string containingFolder)
    {
        var files = Directory.GetFiles(containingFolder, "*.exe");
        foreach (var file in files)
        {
            if (_gameEngines.TryGetValue(Path.GetFileName(file).ToLowerInvariant(), out var gameEngine))
            {
                return gameEngine;
            }
        }

        throw new Exception("Not found :(");
    }
}