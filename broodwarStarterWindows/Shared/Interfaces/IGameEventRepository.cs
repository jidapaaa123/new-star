using Shared.Models;

namespace Shared.Interfaces
{
    public interface IGameEventRepository
    {
        Task<GameEvent> CreateGameEventAsync(GameEvent gameEvent);
        Task<List<GameEvent>> GetGameEventsByMatchAsync(int matchId);
    }
}
