using System.Diagnostics;
using System.Text;
using INIParser;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using TombLauncher.Core.Extensions;
using TombLauncher.Core.Utils;
using TombLauncher.Patchers.Tomb4Plus.Enums;
using TombLauncher.Patchers.Tomb4Plus.Extensions;
using TombLauncher.Patchers.Tomb4Plus.Extractors;
using TombLauncher.Patchers.Tomb4Plus.Models;
using TombLauncher.Patchers.Tomb4Plus.Parsers;

namespace TombLauncher.Patchers.Tomb4Plus.Services;

public class Tomb4PlusFeatureExtractorService
{
    private readonly TrlePatchExtractor _patchExtractor;
    private readonly LeikkuriParser _leikkuriParser;
    private readonly EsseParser _esseParser;
    private readonly FurrSyntaxParser _furrSyntaxParser;
    private readonly ILogger<Tomb4PlusFeatureExtractorService> _logger;

    public Tomb4PlusFeatureExtractorService(TrlePatchExtractor patchExtractor,
        LeikkuriParser leikkuriParser,
        EsseParser esseParser,
        FurrSyntaxParser furrSyntaxParser,
        ILogger<Tomb4PlusFeatureExtractorService> logger)
    {
        _patchExtractor = patchExtractor;
        _leikkuriParser = leikkuriParser;
        _esseParser = esseParser;
        _furrSyntaxParser = furrSyntaxParser;
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

    private async Task ExtractAsync(FeatureExtractorPayload payload, CancellationToken cancellationToken)
    {
        if (!Directory.Exists(payload.InstallDirectory))
            throw new ArgumentException("Invalid directory specified!");

        var exeFile = (payload.ExeFile ?? "tomb4.exe").EnsureEndsWith(".exe");
        var exePath = Path.Combine(payload.InstallDirectory, exeFile);
        if (!File.Exists(exePath))
            throw new ArgumentException("Invalid exe file specified!");

        var exeHash = await CryptoUtils.ComputeMd5Hash(exePath);
        var fileSize = new FileInfo(exePath).Length;
        var isExtendedExeSize = fileSize == Constants.ExeTrepExtendedSize;
        if (isExtendedExeSize)
            _logger.LogInformation("File size {Size} matches TREP extended binary size", fileSize);

        if (Constants.TrleVanillaHashes.Contains(exeHash))
            _logger.LogInformation("The engine was detected as an unextended vanilla build");

        if (Constants.TrleExtendedHashes.Contains(exeHash))
            _logger.LogInformation("The engine was detected as an extended vanilla build.");

        // Attempt to determine if this binary is using memory remapping.
        using var binaryReader = new BinaryReader(File.OpenRead(exePath));
        var isUsingRemappedMemory = binaryReader.ReadByteAt(0x00000122) == Constants.MemoryRemappedFlag;

        _logger.LogInformation("Searching for NextGeneration dll...");
        var trngVersion = DetectNextGenerationDll(payload.InstallDirectory);
        _logger.LogInformation("Searching for effects.bin...");
        var hasEffectsBin = DetectEffectsBin(payload.InstallDirectory);
        
        var patchData = ReadBinaryFile(payload.InstallDirectory, isExtendedExeSize, isUsingRemappedMemory, fileSize, exePath);
        if (patchData.GlobalLevelInfo != null)
            patchData.GlobalLevelInfo.FontInfo ??= new FontInfo();
        _leikkuriParser.ExtractFontDataFromExe(exePath, patchData.GlobalLevelInfo?.FontInfo);
        var esseResult = ReadEsseData(payload, patchData);

        var furrData = await ReadFurrData(payload, cancellationToken, isExtendedExeSize, patchData, exePath, isUsingRemappedMemory);
        var outputModConfig = new GameModConfig();

        var globalInfo = await DetectMetadataIniFile(payload.InstallDirectory, payload.GameMetadata);
        globalInfo.ManifestCompatibilityVersion = Constants.ManifestVersion;
        globalInfo.TrngVersionMajor = 0;
        globalInfo.TrngVersionMinor = 0;
        globalInfo.TrngVersionMaintainence = 0;
        globalInfo.TrngVersionBuild = 0;
        globalInfo.TrngVersionIsPlus = false;
        globalInfo.FurrData = furrData;

        var splitVersion = trngVersion?.FileVersion?.Split(", ") ?? [];

        if (fileSize is not Constants.ExeDefaultSize and not Constants.ExeTrepExtendedSize)
        {
            if (splitVersion.Length == 4)
            {
                globalInfo.TrngVersionMajor = int.Parse(splitVersion[0]);
                globalInfo.TrngVersionMinor = int.Parse(splitVersion[1]);
                globalInfo.TrngVersionMaintainence = int.Parse(splitVersion[2]);
                globalInfo.TrngVersionBuild = int.Parse(new string(splitVersion[3].Where(char.IsDigit).ToArray()));
                globalInfo.TrngVersionIsPlus = trngVersion?.FileVersion?.Contains('+') ?? false;
            }
        }
        else
        {
            globalInfo.TrngFlipEffectsEnabled = false;
            globalInfo.TrngRollingBallExtendedOcb = false;
            globalInfo.TrngStaticsExtendedOcb = false;
            globalInfo.TrngPushableExtendedOcb = false;

            globalInfo.TrepUsingExtendedSaves = isExtendedExeSize && !binaryReader.IsNopAtRange(0x000EF800, 0x000EF922);
        }
        
        if (hasEffectsBin)
        {
            globalInfo.TomoEnableWeatherFlipeffect = true;
            globalInfo.TomoSwapWhitelightForTeleporter = true;
        }

        var globalLevelInfo = patchData.GlobalLevelInfo;

        if (!globalLevelInfo?.AudioInfo?.HasAnyValue() ?? false)
            globalLevelInfo.AudioInfo = null;

        if (!globalLevelInfo?.BarsInfo?.HasAnyValue() ?? false)
            globalLevelInfo.BarsInfo = null;

        if (!globalLevelInfo?.GfxInfo?.HasAnyValue() ?? false)
            globalLevelInfo.GfxInfo = null;

        if (!globalLevelInfo?.EnvironmentInfo?.HasAnyValue() ?? false)
            globalLevelInfo.EnvironmentInfo = null;

        if (!globalLevelInfo?.CreatureInfo?.HasAnyValue() ?? false)
            globalLevelInfo.CreatureInfo = null;

        if (!globalLevelInfo?.CameraInfo?.HasAnyValue() ?? false)
            globalLevelInfo.CameraInfo = null;

        if (!globalLevelInfo?.MiscInfo?.HasAnyValue() ?? false)
            globalLevelInfo.MiscInfo = null;

        if (!globalLevelInfo?.StatInfo?.HasAnyValue() ?? false)
            globalLevelInfo.StatInfo = null;

        if (!globalLevelInfo?.FontInfo?.HasAnyValue() ?? false)
            globalLevelInfo.FontInfo = null;

        if (!globalLevelInfo?.LaraInfo?.HasAnyValue() ?? false)
            globalLevelInfo.LaraInfo = null;

        if (!globalLevelInfo?.ObjectsInfo?.HasAnyValue() ?? false)
            globalLevelInfo.ObjectsInfo = null;

        outputModConfig.GlobalInfo = globalInfo;
        if (globalLevelInfo?.HasAnyValue() ?? false)
        {
            outputModConfig.GlobalLevelInfo = globalLevelInfo;
        }

        if (esseResult.IsNotNullOrEmpty())
            outputModConfig.Levels = esseResult;

        var jsonData = JsonConvert.SerializeObject(outputModConfig, Formatting.Indented,
            new JsonSerializerSettings()
            {
                NullValueHandling = NullValueHandling.Ignore,
                ContractResolver = new DefaultContractResolver() { NamingStrategy = new SnakeCaseNamingStrategy() }
            });

        var jsonPath = Path.Combine(payload.InstallDirectory, "game_mod_config.json");

        await File.WriteAllTextAsync(jsonPath, jsonData, cancellationToken);

        var portableSettingsFileName = Path.Combine(payload.InstallDirectory, "portable.txt");
        await File.WriteAllTextAsync(portableSettingsFileName, "", cancellationToken);
    }

    private List<LevelData> ReadEsseData(FeatureExtractorPayload payload, TrlePatchExecutorOutput patchData)
    {
        List<LevelData> esseResult = [];

        if (patchData.MetaInfo?.EsseFileLoading ?? false)
        {
            var essePath = Path.Combine(payload.InstallDirectory, "script2.dat");
            _logger.LogInformation("Searching for {EssePath}", essePath);
            if (File.Exists(essePath))
            {
                _logger.LogInformation("Found eSSe script file at {EssePath}", essePath);
                esseResult = _esseParser.ReadBinaryFile(essePath, patchData) ?? [];
            }
            else
            {
                _logger.LogInformation("No eSSe script file found.");
            }
        }

        return esseResult;
    }

    private async Task<FurrData?> ReadFurrData(FeatureExtractorPayload payload, CancellationToken cancellationToken,
        bool isExtendedExeSize, TrlePatchExecutorOutput patchData, string exePath, bool isUsingRemappedMemory)
    {
        FurrData? furrData = null;
        if (isExtendedExeSize)
        {
            if (patchData.MetaInfo?.FurrSupport ?? false)
            {
                _logger.LogInformation("Scanning for FURR modifications in exe file...");
                furrData = await _furrSyntaxParser.ReadExeFile(payload.SyntaxFile, exePath, isUsingRemappedMemory, cancellationToken);
            }
            else
            {
                _logger.LogInformation("FURR support not detected. Skipping.");
            }
        }
        else
        {
            _logger.LogInformation("Unknown EXE file size, skipping FURR extraction in exe file...");
        }

        return furrData;
    }


    private TrlePatchExecutorOutput ReadBinaryFile(string installDirectory, bool isExtendedExeSize,
        bool isUsingRemappedMemory, long fileSize,
        string exePath)
    {
        var patchesPath = Path.Combine(installDirectory, "patches.bin");
        string fileToParse;
        bool isExtendedFileSize;
        var isUsingRemappedMemoryActualValue = false;
        var patchType = PatchBinaryType.FlepExternalBinary;
        if (File.Exists(patchesPath))
        {
            _logger.LogInformation("Found FLEP patches file at {PatchesPath}", patchesPath);
            fileToParse = patchesPath;
            isExtendedFileSize = true;
        }
        else
        {
            fileToParse = exePath;
            isExtendedFileSize = isExtendedExeSize;
            isUsingRemappedMemoryActualValue = isUsingRemappedMemory;
            _logger.LogInformation("External patch binary not found...");
            if (fileSize is Constants.ExeTrepExtendedSize or Constants.ExeDefaultSize)
            {
                _logger.LogInformation("Scanning for TREP modifications in exe file...");
                patchType = PatchBinaryType.TrepExe;
            }
            else
            {
                _logger.LogInformation("Scanning for FLEP modifications in exe file...");
                patchType = PatchBinaryType.FlepExe;
            }
        }

        return _patchExtractor.ReadBinaryFile(fileToParse, isExtendedFileSize, isUsingRemappedMemoryActualValue, patchType);
    }
}