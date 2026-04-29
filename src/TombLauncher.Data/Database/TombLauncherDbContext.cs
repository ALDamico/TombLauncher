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
    public DbSet<PlaySession> PlaySession { get; set; }
    public DbSet<GameLink> GameLink { get; set; }
    public DbSet<GameHashes> GameHashes { get; set; }
    public DbSet<SavegameMetadata> SavegameMetadata { get; set; }
    public DbSet<GameEnvironmentVariable> GameEnvironmentVariables { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Game>()
            .HasOne(g => g.InstalledFromLink)
            .WithMany()
            .HasForeignKey("InstalledFromLinkId")
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<GameEnvironmentVariable>()
            .HasOne(e => e.Game)
            .WithMany(g => g.EnvironmentVariables)
            .HasForeignKey(e => e.GameId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}