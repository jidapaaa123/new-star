using BWAPI.NET;
using Shared.Interfaces;
using Shared.Models;

public interface IConstructionManager
{
    void Reset();
    List<ConstructionOrder> PendingConstructionOrders { get; }
    int GetReservedGas();
    int GetReservedMinerals();
    Materials GetReservedMaterials();
    bool IsWorkerAssignedToConstruction(IMyUnit worker);
    void RegisterOrder(UnitType type, IMyUnit worker, TilePosition tilePosition, bool isFromBuildOrder);
    void RegisterOrder(UnitType addonType, IMyUnit parentUnit, bool isFromBuildOrder);
    ConstructionOrder RemoveWorkerConstructionOrder(IMyUnit worker);
    int? GetPendingWorkerId();
    ConstructionOrder? OrderOfWorker(IMyUnit worker);
    void RecalibrateWorker();
    ConstructionOrder? OrderOfAddonType(UnitType unitType);
    ConstructionOrder RemoveAddonConstructionOrder(UnitType buildingType);
}