using Microsoft.EntityFrameworkCore;
using TombLauncher.Data.Models;

namespace TombLauncher.Data.Database;

public class TombLauncherDbContext : DbContext
{
    public TombLauncherDbContext(DbContextOptions<TombLauncherDbContext> options) : base(options)
    {
    }

    public DbSet<Game> Games { get; set; }
    public DbSet<AppCrash> AppCrashes { get; set; }
    public DbSet<FileBackup> FileBackups { get; set; }
    public DbSet<PlaySession> PlaySessions { get; set; }
    public DbSet<GameLink> GameLinks { get; set; }
    public DbSet<GameHashes> GameHashes { get; set; }
    public DbSet<SavegameMetadata> SavegameMetadata { get; set; }
}