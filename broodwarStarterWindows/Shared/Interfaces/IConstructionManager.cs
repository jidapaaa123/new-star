using BWAPI.NET;
using Shared.Interfaces;
using Shared.Models;

public interface IConstructionManager
{
    ConstructionOrder? PendingConstructionOrder { get; }
    int GetReservedGas();
    int GetReservedMinerals();
    Materials GetReservedMaterials();
    void RegisterOrder(UnitType type, IMyUnit worker, TilePosition tilePosition, bool isFromBuildOrder);
    void RegisterOrder(UnitType addonType, IMyUnit parentUnit, bool isFromBuildOrder);
    ConstructionOrder RemovePendingConstructionOrder();
    void RecalibrateWorker(IGameData gameData);
}