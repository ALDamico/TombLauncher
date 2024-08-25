using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using HarfBuzzSharp;
using TombLauncher.Dto;

namespace TombLauncher.Installers;

public class GameFileHashCalculator
{
    public GameFileHashCalculator(IEnumerable<string> extensionsToCheck)
    {
        _extensions = extensionsToCheck.ToHashSet();
    }

    private readonly HashSet<string> _extensions;
    /*
     // TODO Inject from outside
    private HashSet<string> _extensions = new HashSet<string>()
    {
        ".tr4",
        ".pak",
        ".tr2",
        ".sfx",
        ".dat",
        ".phd"
    };
    */
    public List<string> GetFilesToProcess(string dataFolder)
    {
        return Directory.GetFiles(dataFolder, "*.*", SearchOption.AllDirectories)
            .Where(f => _extensions.Any(f.EndsWith)).ToList();
    }

    public List<GameHashDto> CalculateHashes(string dataFolder, int gameId = default)
    {
        var hashes = new List<GameHashDto>();
        var files = GetFilesToProcess(dataFolder);
        foreach (var file in files)
        {
            var relativePath = Path.GetRelativePath(dataFolder, file);
            var md5 = MD5.Create();
            using (var stream = File.OpenRead(file))
            {
                var md5Hash = md5.ComputeHash(stream);
                var md5String = BitConverter.ToString(md5Hash).Replace("-", "").ToLowerInvariant();
                var dto = new GameHashDto()
                {
                    FileName = relativePath,
                    GameId = gameId,
                    Md5Hash = md5String
                };
                hashes.Add(dto);
            }
        }

        return hashes;
    }
}