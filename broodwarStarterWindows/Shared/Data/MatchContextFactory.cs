using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Shared.Data
{
    public class MatchContextFactory : IDesignTimeDbContextFactory<MatchContext>
    {
        public MatchContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<MatchContext>();
            var dbPath = Path.Combine(AppContext.BaseDirectory, "matches.db");
            optionsBuilder.UseSqlite($"Data Source={dbPath}");
            return new MatchContext(optionsBuilder.Options);
        }
    }
}