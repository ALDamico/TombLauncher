using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using HarfBuzzSharp;
using Ionic.Zip;
using TombLauncher.Dto;
using TombLauncher.Utils;

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
        if (Directory.Exists(dataFolder))
        {
            return ProcessFolder(dataFolder, gameId);
        }

        if (ZipFile.IsZipFile(dataFolder))
        {
            return ProcessZipFile(dataFolder, gameId);
        }


        throw new InvalidOperationException("Provided path isn't a directory or a zip file");
    }

    private List<GameHashDto> ProcessZipFile(string dataFolder, int gameId)
    {
        var hashes = new List<GameHashDto>();
        using var zipFile = new ZipFile(dataFolder);
        var entriesToProcess = zipFile.Entries.Where(e => _extensions.Any(ex => e.FileName.EndsWith(ex))).ToList();
        foreach (var entry in entriesToProcess)
        {
            var relativePath = PathUtils.NormalizePath(entry.FileName);
            var md5 = MD5.Create();

            using var stream = entry.OpenReader();
            var dto = GetGameHashDto(gameId, md5, stream, relativePath);
            hashes.Add(dto);
        }

        return hashes;
    }

    private List<GameHashDto> ProcessFolder(string dataFolder, int gameId)
    {
        var hashes = new List<GameHashDto>();
        var files = GetFilesToProcess(dataFolder);
        foreach (var file in files)
        {
            var relativePath = PathUtils.NormalizePath(Path.GetRelativePath(dataFolder, file));
            var md5 = MD5.Create();
            using (var stream = File.OpenRead(file))
            {
                var dto = GetGameHashDto(gameId, md5, stream, relativePath);
                hashes.Add(dto);
            }
        }

        return hashes;
    }

    private static GameHashDto GetGameHashDto(int gameId, MD5 md5, Stream stream, string relativePath)
    {
        var md5Hash = md5.ComputeHash(stream);
        var md5String = BitConverter.ToString(md5Hash).Replace("-", "").ToLowerInvariant();
        var dto = new GameHashDto()
        {
            FileName = relativePath,
            GameId = gameId,
            Md5Hash = md5String
        };
        return dto;
    }
}