using System.Diagnostics;
using System.Text;
using INIParser;
using Microsoft.Extensions.Logging;
using TombLauncher.Patchers.Tomb4Plus.Models;

namespace TombLauncher.Patchers.Tomb4Plus.Services;

public class Tomb4PlusFeatureExtractorService
{
    private readonly ILogger<Tomb4PlusFeatureExtractorService> _logger;

    public Tomb4PlusFeatureExtractorService(ILogger<Tomb4PlusFeatureExtractorService> logger)
    {
        _logger = logger;
    }
    
    private FileVersionInfo? DetectNextGenerationDll(string path)
    {
        var ngDllPath = Path.Combine(path, "Tomb_NextGeneration.dll");
        if (!File.Exists(ngDllPath))
        {
            _logger.LogInformation("No Tomb_NextGeneration.dll at {Path}. Returning", ngDllPath);
            return null;
        }

        return FileVersionInfo.GetVersionInfo(ngDllPath);
    }

    private bool DetectEffectsBin(string path)
    {
        var effectsBinPath = Path.Combine(path, "effects.bin");
        return File.Exists(effectsBinPath);
    }

    private void ParseIniFile(string filePath, GlobalInfo globalInfo)
    {
        var iniFile = new IniFile(filePath);
        globalInfo.GameName = iniFile["Game Information", "game_name"];
        globalInfo.Authors = iniFile["Game Information", "authors"];
        globalInfo.ReleaseDate = iniFile["Game Information", "release_date"];
        globalInfo.GameUserDirName = iniFile["Game Information", "game_user_dir_name"];
    }

    private async Task<GlobalInfo> DetectMetadataIniFile(string path, BasicGameMetadata data)
    {
        var metadataIniPath = Path.Combine(path, "metadata.ini");
        if (!File.Exists(metadataIniPath))
        {
            var iniFile = new IniFile(metadataIniPath)
            {
                ["Game Information", "game_name"] = data.GameName,
                ["Game Information", "authors"] = data.Authors,
                ["Game Information", "release_date"] = data.ReleaseDate.ToString("dd/MM/yyyy"),
                ["Game Information", "game_user_dir_name"] = data.GameUserDirName
            };
            await iniFile.WriteFileAsync(metadataIniPath, Encoding.UTF8);
        }

        var globalInfo = new GlobalInfo();
        ParseIniFile(metadataIniPath, globalInfo);
        return globalInfo;
    }
}