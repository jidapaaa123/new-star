using Microsoft.EntityFrameworkCore;
using Shared.Data;
using Shared.Interfaces;
using Shared.Models;

namespace Shared.Services
{
    public class MatchRepository : IMatchRepository
    {
        private readonly IDbContextFactory<MatchContext> _contextFactory;

        public MatchRepository(IDbContextFactory<MatchContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task<IEnumerable<Match>> GetAllMatchesAsync()
        {
            using (var context = _contextFactory.CreateDbContext())
            {
                return await Task.FromResult(context.Matches.ToList());
            }
        }

        public async Task<Match?> GetMatchByIdAsync(int id)
        {
            using (var context = _contextFactory.CreateDbContext())
            {
                return await Task.FromResult(context.Matches.FirstOrDefault(m => m.Id == id));
            }
        }

        public async Task<MatchStatistics> GetStatisticsAsync()
        {
            using (var context = _contextFactory.CreateDbContext())
            {
                var matches = context.Matches.ToList();

                if (matches.Count == 0)
                {
                    return new MatchStatistics();
                }

                var completedMatches = matches.Where(m => m.EndTime.HasValue).ToList();
                var wonMatches = matches.Count(m => m.Result == "Win");
                var lostMatches = matches.Count(m => m.Result == "Loss");

                var statistics = new MatchStatistics
                {
                    TotalMatches = matches.Count,
                    WonMatches = wonMatches,
                    LostMatches = lostMatches,
                    WinRate = matches.Count > 0 ? (double)wonMatches / matches.Count : 0,
                    AverageDuration = completedMatches.Count > 0
                        ? TimeSpan.FromMilliseconds(completedMatches
                            .Where(m => m.EndTime.HasValue)
                            .Average(m => (m.EndTime!.Value - m.StartTime).TotalMilliseconds))
                        : TimeSpan.Zero,
                    ExpansionRate = matches.Count > 0 ? (double)matches.Count(m => m.DidExpand) / matches.Count : 0,
                    TotalUpgradesCompleted = matches.Sum(m => m.UpgradesCompleted),
                    AverageFinalWorkerCount = matches.Count > 0 ? matches.Average(m => m.FinalWorkerCount) : 0,
                    AverageFinalMilitaryCount = matches.Count > 0 ? matches.Average(m => m.FinalMilitaryCount) : 0,
                    AverageFinalMinerals = matches.Count > 0 ? matches.Average(m => m.FinalMinerals) : 0,
                    AverageFinalGas = matches.Count > 0 ? matches.Average(m => m.FinalGas) : 0
                };

                return await Task.FromResult(statistics);
            }
        }

        public async Task<Match> CreateMatchAsync(Match match)
        {
            using (var context = _contextFactory.CreateDbContext())
            {
                context.Matches.Add(match);
                await context.SaveChangesAsync();
                return match;
            }
        }

        public async Task<Match> UpdateMatchAsync(int id, Match match)
        {
            using (var context = _contextFactory.CreateDbContext())
            {
                var existingMatch = context.Matches.FirstOrDefault(m => m.Id == id);
                if (existingMatch == null)
                {
                    throw new KeyNotFoundException($"Match with ID {id} not found.");
                }

                existingMatch.EndTime = match.EndTime;
                existingMatch.FinalWorkerCount = match.FinalWorkerCount;
                existingMatch.FinalMilitaryCount = match.FinalMilitaryCount;
                existingMatch.FinalMinerals = match.FinalMinerals;
                existingMatch.FinalGas = match.FinalGas;
                existingMatch.DidExpand = match.DidExpand;
                existingMatch.UpgradesCompleted = match.UpgradesCompleted;
                existingMatch.Result = match.Result;

                context.Matches.Update(existingMatch);
                await context.SaveChangesAsync();
                return existingMatch;
            }
        }
    }
}

