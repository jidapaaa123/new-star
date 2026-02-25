using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Shouldly;
using Shared.Data;
using Shared.Models;
using Shared.Services;
using Xunit;

namespace TestProject1
{
    public class TestDatabaseFixture : IAsyncLifetime
    {
        private DbContextOptions<MatchContext> _options;
        private Microsoft.Data.Sqlite.SqliteConnection _connection;

        public async Task InitializeAsync()
        {
            // Create a persistent connection for in-memory SQLite database
            // This is necessary because in-memory SQLite databases are destroyed
            // when all connections to them are closed
            _connection = new Microsoft.Data.Sqlite.SqliteConnection("Data Source=:memory:");
            await _connection.OpenAsync();

            _options = new DbContextOptionsBuilder<MatchContext>()
                .UseSqlite(_connection)
                .Options;

            // Create the database schema
            using (var context = new MatchContext(_options))
            {
                await context.Database.EnsureCreatedAsync();
            }
        }

        public async Task DisposeAsync()
        {
            if (_connection != null)
            {
                await _connection.CloseAsync();
                _connection.Dispose();
            }
        }

        public DbContextOptions<MatchContext> GetOptions() => _options;
    }

    public class MatchRepositoryTests : IClassFixture<TestDatabaseFixture>
    {
        private readonly TestDatabaseFixture _fixture;

        public MatchRepositoryTests(TestDatabaseFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public async Task GetAllMatchesAsync_ReturnsAllMatches()
        {
            // ARRANGE
            var match1 = new Shared.Models.Match
            {
                Id = 1,
                StartTime = DateTime.UtcNow.AddHours(-2),
                EndTime = DateTime.UtcNow.AddHours(-1),
                Result = "Win",
                FinalWorkerCount = 20,
                FinalMinerals = 600,
                FinalGas = 350,
                FinalMilitaryCount = 15
            };

            var match2 = new Shared.Models.Match
            {
                Id = 2,
                StartTime = DateTime.UtcNow.AddHours(-1),
                EndTime = DateTime.UtcNow,
                Result = "Loss",
                FinalWorkerCount = 18,
                FinalMinerals = 400,
                FinalGas = 200,
                FinalMilitaryCount = 12
            };

            // Insert test data
            using (var context = new MatchContext(_fixture.GetOptions()))
            {
                context.Matches.Add(match1);
                context.Matches.Add(match2);
                await context.SaveChangesAsync();
            }

            // Create repository with properly initialized context
            var contextFactory = new TestDbContextFactory(_fixture.GetOptions());
            var repository = new MatchRepository(contextFactory);

            // ACT
            var matches = await repository.GetAllMatchesAsync();

            // ASSERT
            matches.ShouldNotBeEmpty();
            matches.Count().ShouldBe(2);
            matches.First().Result.ShouldBe("Win");
            matches.Last().Result.ShouldBe("Loss");
        }
    }

    /// <summary>
    /// Helper class to provide DbContextFactory for testing
    /// </summary>
    public class TestDbContextFactory : IDbContextFactory<MatchContext>
    {
        private readonly DbContextOptions<MatchContext> _options;

        public TestDbContextFactory(DbContextOptions<MatchContext> options)
        {
            _options = options;
        }

        public MatchContext CreateDbContext()
        {
            return new MatchContext(_options);
        }
    }
}
