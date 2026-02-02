namespace MobileApp.Services
{
    public interface IBotControlService
    {
        Task<string?> HelloWorldAsync();
        Task<bool> BuildBunkerAtChokepointAsync();
        Task<bool> BuildSupplyDepotAtChokepointAsync();
        Task<bool> ToggleStrategyAsync();
        Task<bool> ToggleAttackEnemyBaseAsync();
        Task<bool> ScoutMapAsync();
        Task<bool> TogglePauseBot();
    }
}