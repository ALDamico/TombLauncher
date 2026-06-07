using System.ComponentModel;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using TombLauncher.Ai.Models;
using TombLauncher.Core.Dtos;
using TombLauncher.Data.Database.Repositories;

namespace TombLauncher.Ai.Plugins;

public class SavegamePlugin
{
    private readonly ISavegameRepository _savegameRepository;
    private readonly ILogger<SavegamePlugin> _logger;

    public SavegamePlugin(TroubleshootingContext troubleshootingContext, ISavegameRepository savegameRepository, ILogger<SavegamePlugin> logger)
    {
        _savegameRepository = savegameRepository;
        _logger = logger;
        TroubleshootingContext = troubleshootingContext;
    }
    public TroubleshootingContext TroubleshootingContext { get; }
    
    [KernelFunction]
    [Description("Use this function to retrieve the list of savegames for the current game")]
    public async Task<List<FileBackupDto>> FetchSavegames([Description("The number of savegames to retrieve. If null, retrieves all savegames for the current game.")]int? quantity = 10)
    {
        return await _savegameRepository.GetSavegamesByGameId(TroubleshootingContext.GameId, quantity);
    }

    [KernelFunction]
    [Description("Use this function to restore a savegame. Ask confirmation from the user before calling this function")]
    public async Task<string> RestoreSavegame(int savegameId)
    {
        var savegame = _savegameRepository.GetSavegameById(savegameId);

        if (savegame == null)
            return $"Savegame with id {savegameId} not found. No restore performed.";

        if (savegame.GameId != TroubleshootingContext.GameId)
            return "Error: The retrieved savegame does not belong to the current game.";

        try
        {
            await File.WriteAllBytesAsync(savegame.FileName, savegame.Data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to write savegame");
            return $"Error: An exception occurred while writing {Path.GetFileName(savegame.FileName)}";
        }

        return $"Savegame successfully restored to {savegame.FileName}";
    }
}