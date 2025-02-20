using Microsoft.EntityFrameworkCore;
using TombLauncher.Data.Models;

namespace TombLauncher.Data.Database;

public class TombLauncherDbContext : DbContext
{
    public TombLauncherDbContext(DbContextOptions<TombLauncherDbContext> options) : base(options)
    {
    }

    internal DbSet<Game> Games { get; set; }
    internal DbSet<AppCrash> AppCrashes { get; set; }
    internal DbSet<FileBackup> FileBackups { get; set; }
}