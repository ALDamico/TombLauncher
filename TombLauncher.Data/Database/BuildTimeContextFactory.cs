using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace TombLauncher.Data.Database;

internal class BuildTimeContextFactory : IDesignTimeDbContextFactory<TombLauncherDbContext>
{
    public TombLauncherDbContext CreateDbContext(string[] args)
    {
        var dbContextBuilder = new DbContextOptionsBuilder<TombLauncherDbContext>();
        var connString = "Data Source=./TombLauncher.db";
        dbContextBuilder.UseSqlite(connString);
        return new TombLauncherDbContext(dbContextBuilder.Options);
    }
}