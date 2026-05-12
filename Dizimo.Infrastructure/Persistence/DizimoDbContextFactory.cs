using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Dizimo.Infrastructure.Persistence;

public class DizimoDbContextFactory : IDesignTimeDbContextFactory<DizimoDbContext>
{
    public DizimoDbContext CreateDbContext(string[] args)
    {
        var dbPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "dizimo.db"
        );
        var optionsBuilder = new DbContextOptionsBuilder<DizimoDbContext>();
        optionsBuilder.UseSqlite($"Data Source={dbPath}");
        return new DizimoDbContext(optionsBuilder.Options);
    }
}