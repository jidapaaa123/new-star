namespace MobileApp.Services
{
    public interface IBotControlService
    {
        Task<string?> HelloWorldAsync();
        Task<bool> BuildBunkerAtChokepointAsync();
        Task<bool> BuildSupplyDepotAtChokepointAsync();
        Task<bool> ToggleStrategyAsync();
        Task<bool> ChangeStrategyAsync(string strategy);
        Task<bool> ToggleAttackEnemyBaseAsync();
        Task<bool> ScoutMapAsync();
        Task<bool> TogglePauseBot();
        Task<bool> ExpandAsync();
        Task<int?> GetLatestMatchIdAsync();
        Task<List<(string EventType, string Description, DateTime Timestamp)>?> GetMatchEventsAsync(int matchId);
    }
}