using Microsoft.EntityFrameworkCore;
using TombLauncher.Data.Models;

namespace TombLauncher.Data.Database;

public class TombLauncherDbContext : DbContext
{
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite("Data Source=./TombLauncher.db");
    }

    internal DbSet<Game> Games { get; set; }
    internal DbSet<AppCrash> AppCrashes { get; set; }
    internal DbSet<ApplicationSetting> Settings { get; set; }
}