using Microsoft.EntityFrameworkCore;
using TombLauncher.Models.Models;

namespace TombLauncher.Database;

public class TombLauncherDbContext : DbContext
{
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite("Data Source=./TombLauncher.db");
    }

    internal DbSet<Game> Games { get; set; }
}