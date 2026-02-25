using Shared.Data;
using Shared.Interfaces;
using Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Shared.Services
{
    public class GameEventRepository : IGameEventRepository
    {
        private readonly IDbContextFactory<MatchContext> _contextFactory;

        public GameEventRepository(IDbContextFactory<MatchContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task<GameEvent> CreateGameEventAsync(GameEvent gameEvent)
        {
            using (var context = _contextFactory.CreateDbContext())
            {
                context.GameEvents.Add(gameEvent);
                await context.SaveChangesAsync();
                return gameEvent;
            }
        }

        public async Task<List<GameEvent>> GetGameEventsByMatchAsync(int matchId)
        {
            using (var context = _contextFactory.CreateDbContext())
            {
                return await context.GameEvents
                    .Where(e => e.MatchId == matchId)
                    .OrderBy(e => e.Timestamp)
                    .ToListAsync();
            }
        }
    }
}
