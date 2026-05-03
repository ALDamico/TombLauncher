using Microsoft.EntityFrameworkCore;
using TombLauncher.Contracts.Enums;
using TombLauncher.Core.Dtos;
using TombLauncher.Data.Mapping;
using TombLauncher.Data.Models;

namespace TombLauncher.Data.Database.Repositories;

public interface ISavegameRepository
{
    void BackupSavegames(int gameId, GameEngine engine, List<SavegameBackupDto> dtos, int? numberOfVersionsToKeep);
    Task<List<FileBackupDto>> GetSavegamesByGameId(int gameId);
    Task<List<string?>> GetSavegameMd5HashesByGameId(int gameId);
    Task UpdateSavegameStartOfLevel(FileBackupDto targetSaveGame);
    Task DeleteFileBackupById(int id);
    void DeleteFileBackupsByGameId(int gameId, IEnumerable<FileType>? fileTypes = null);
    SavegameBackupDto? GetSavegameById(int id);
    Task<List<SavegameBackupDto>> GetSavegameBackups();
    Task SyncSavegameMetadata(IEnumerable<SavegameBackupDto> dtos);
    Task Save();
}

public class SavegameRepository : ISavegameRepository
{
    private readonly EfRepository<FileBackup> _backups;
    private readonly EfRepository<SavegameMetadata> _savegameMetadata;
    private readonly TombLauncherDbContext _dbContext;
    private readonly FileBackupMapper _mapper;

    public SavegameRepository(TombLauncherDbContext dbContext, FileBackupMapper mapper)
    {
        _dbContext = dbContext;
        _mapper = mapper;
        _backups = new EfRepository<FileBackup>(dbContext);
        _savegameMetadata = new EfRepository<SavegameMetadata>(dbContext);
    }

    public void BackupSavegames(int gameId, GameEngine engine, List<SavegameBackupDto> dtos, int? numberOfVersionsToKeep)
    {
        dtos.ForEach(f => f.GameId = gameId);
        dtos.ForEach(f => f.GameEngine = engine);
        var entitiesToPersist = _mapper.ToFileBackups(dtos);
        foreach (var entity in entitiesToPersist)
        {
            _backups.Insert(entity);
            _savegameMetadata.Upsert(entity.SavegameMetadata!);
        }

        if (numberOfVersionsToKeep.HasValue)
        {
            var groups = _backups.GetAll().Include(b => b.SavegameMetadata).Where(b => b.GameId == gameId)
                .OrderByDescending(b => b.BackedUpOn)
                .GroupBy(b => b.SavegameMetadata!.SlotNumber);
            foreach (var group in groups)
            {
                var lastDate = group.Select(g => g).Take(numberOfVersionsToKeep.Value).LastOrDefault()?.BackedUpOn;
                _backups.GetAll().Where(b => b.GameId == gameId).Include(b => b.SavegameMetadata)
                    .Where(b => b.BackedUpOn < lastDate).ExecuteDelete();
            }
        }
    }

    public async Task<List<FileBackupDto>> GetSavegamesByGameId(int gameId)
    {
        var backups = _backups.GetAll().Where(f => f.FileType == FileType.Savegame || f.FileType == FileType.SavegameStartOfLevel)
            .Where(f => f.GameId == gameId)
            .OrderByDescending(f => f.BackedUpOn);

        var entities = await backups.ToListAsync();

        return _mapper.ToDtos(entities);
    }

    public async Task<List<string?>> GetSavegameMd5HashesByGameId(int gameId)
    {
        return await _backups.GetAll().Where(f => f.FileType == FileType.Savegame || f.FileType == FileType.SavegameStartOfLevel)
            .Where(f => f.GameId == gameId).Select(sg => sg.Md5).ToListAsync();
    }

    public async Task UpdateSavegameStartOfLevel(FileBackupDto targetSaveGame)
    {
        var entityToUpdate = _backups.GetEntityById(targetSaveGame.Id);
        if (entityToUpdate == null)
            throw new InvalidOperationException();
        entityToUpdate.FileType = targetSaveGame.FileType;
        _backups.Update(entityToUpdate);
        await Save();
    }

    public async Task DeleteFileBackupById(int id)
    {
        if (_backups.Delete(id))
            await Save();
    }

    public void DeleteFileBackupsByGameId(int gameId, IEnumerable<FileType>? fileTypes = null)
    {
        var byGameId = _backups.Get(b => b.GameId == gameId);
        if (fileTypes != null)
        {
            byGameId = byGameId.Where(b => fileTypes.Contains(b.FileType));
        }

        byGameId.ExecuteDelete();
    }

    public SavegameBackupDto? GetSavegameById(int id)
    {
        var entity = _backups.GetEntityById(id);
        return _mapper.ToSavegameBackupDto(entity);
    }

    public async Task<List<SavegameBackupDto>> GetSavegameBackups()
    {
        var entities = await _backups.Get().Include(b => b.SavegameMetadata)
            .Include(b => b.Game)
            .Where(b => b.FileType == FileType.Savegame || b.FileType == FileType.SavegameStartOfLevel)
            .ToListAsync();

        return _mapper.ToSavegameBackupDtos(entities);
    }

    public async Task SyncSavegameMetadata(IEnumerable<SavegameBackupDto> dtos)
    {
        var savegameBackupDtos = dtos as SavegameBackupDto[] ?? dtos.ToArray();
        var idsToFind = savegameBackupDtos.Select(dto => dto.Id);
        var mappedEntities = _backups.GetAll().Include(b => b.SavegameMetadata)
            .Join(idsToFind, b => b.Id, i => i, (backup, i) => backup).ToList();
        var lookup = savegameBackupDtos.ToDictionary(dto => dto.Id);

        foreach (var entity in mappedEntities)
        {
            var backup = lookup[entity.Id];
            var savegameMetadata = entity.SavegameMetadata;
            if (savegameMetadata != null)
            {
                savegameMetadata.LevelName = backup.LevelName;
                savegameMetadata.SlotNumber = backup.SlotNumber;
                savegameMetadata.SaveNumber = backup.SaveNumber;
            }
            
            entity.Md5 = backup.Md5;
            entity.BackedUpOn = DateTime.Now;

            _backups.Update(entity);
        }
        await Save();
    }

    public async Task Save()
    {
        await _dbContext.SaveChangesAsync();
    }
}
