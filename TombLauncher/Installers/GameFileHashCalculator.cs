using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using ICSharpCode.SharpZipLib.Zip;
using TombLauncher.Core.Dtos;
using TombLauncher.Core.Utils;
using TombLauncher.Extensions;
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

    public async Task<List<GameHashDto>> CalculateHashes(string dataFolder, int gameId = default)
    {
        if (Directory.Exists(dataFolder))
        {
            return await ProcessFolder(dataFolder, gameId);
        }

        using (var zipManager = new ZipManager(dataFolder))
        {
            return await ProcessZipFile(zipManager, gameId);
        }


        throw new InvalidOperationException("Provided path isn't a directory or a zip file");
    }

    private async Task<List<GameHashDto>> ProcessZipFile(ZipManager zipFile, int gameId)
    {
        var hashes = new List<GameHashDto>();
        
        var entriesToProcess = zipFile.GetEntries().Where(e => _extensions.Any(ex => e.Name.EndsWith(ex))).ToList();
        foreach (var entry in entriesToProcess)
        {
            
            var relativePath = PathUtils.NormalizePath(entry.Name);
            var stream = zipFile.GetInputStream(entry);
            var dto = await GetGameHashDto(gameId, stream, relativePath);
            hashes.Add(dto);
        }

        return hashes;
    }

    private async Task<List<GameHashDto>> ProcessFolder(string dataFolder, int gameId)
    {
        var hashes = new List<GameHashDto>();
        var files = GetFilesToProcess(dataFolder);
        foreach (var file in files)
        {
            var relativePath = PathUtils.NormalizePath(Path.GetRelativePath(dataFolder, file));
            await using var stream = File.OpenRead(file);
            var dto = await GetGameHashDto(gameId, stream, relativePath);
            hashes.Add(dto);
        }

        return hashes;
    }

    private static async Task<GameHashDto> GetGameHashDto(int gameId, Stream stream, string relativePath)
    {
        var md5String = await Md5Utils.ComputeMd5Hash(stream);
        var dto = new GameHashDto()
        {
            FileName = relativePath,
            GameId = gameId,
            Md5Hash = md5String
        };
        return dto;
    }
}