using BWAPI.NET;
using Shared.Interfaces;
using Shared.Models;

public class ConstructionOrder
{
    public UnitType BuildingType { get; set; }
    public IMyUnit? Worker { get; set; } = null;
    public Materials Costs { get; set; } = new Materials();
    public TilePosition TilePosition { get; set; }
    public bool IsFromBuildOrder { get; set; } = false;
    public IMyUnit? ParentUnit { get; set; }
}

public class ConstructionManager : IConstructionManager
{
    public List<ConstructionOrder> PendingConstructionOrders { get; private set; } = new List<ConstructionOrder>();

    public void Reset()
    {
        PendingConstructionOrders.Clear();
    }

    /// <summary>
    /// Adds to PendingConstructionOrders and commands the worker to build.
    /// </summary>
    /// <param name="type"></param>
    /// <param name="worker"></param>
    /// <param name="tilePosition"></param>
    public void RegisterOrder(UnitType type, IMyUnit worker, TilePosition tilePosition, bool isFromBuildOrder)
    {
        PendingConstructionOrders.Add(new ConstructionOrder
        {
            BuildingType = type,
            ParentUnit = null,
            Worker = worker,
            Costs = new Materials
            {
                Minerals = type.MineralPrice(),
                Gas = type.GasPrice()
            },
            TilePosition = tilePosition,
            IsFromBuildOrder = isFromBuildOrder
        });

        if (worker.IsCarryingMaterial())
        {
            worker.ReturnCargo();
        }

        worker.Build(type, tilePosition);
    }

    public void RecalibrateWorker()
    {
        if (PendingConstructionOrders.Count == 0)
            return;

        var type = PendingConstructionOrders[0].BuildingType;
        var worker = PendingConstructionOrders[0].Worker;
        var tilePosition = PendingConstructionOrders[0].TilePosition;
        worker.Build(type, tilePosition);
    }

    public ConstructionOrder RemoveWorkerConstructionOrder(IMyUnit worker)
    {
        var order = OrderOfWorker(worker);
        if (order == null)
            throw new ArgumentException($"No construction order attached to Worker#{worker.GetID()}");

        // RemoveAll works because a worker can only be assigned to one construction at a time
        PendingConstructionOrders.RemoveAll(o => o == order);
        return order;
    }


    /// <summary>
    /// Doesn't add to PendingConstructionOrders, just commands the parent unit to build the addon.
    /// </summary>
    /// <param name="addonType"></param>
    /// <param name="parentUnit"></param>
    public void RegisterOrder(UnitType addonType, IMyUnit parentUnit, bool isFromBuildOrder)
    {
        parentUnit.BuildAddon(addonType);
    }

    public int GetReservedMinerals() => PendingConstructionOrders.Sum(o => o.Costs.Minerals);
    public int GetReservedGas() => PendingConstructionOrders.Sum(o => o.Costs.Gas);
    public Materials GetReservedMaterials() => new Materials
    {
        Minerals = GetReservedMinerals(),
        Gas = GetReservedGas()
    };
    public bool IsWorkerAssignedToConstruction(IMyUnit worker) => PendingConstructionOrders.Any(o => o.Worker?.GetID() == worker.GetID());
    public int? GetPendingWorkerId() => PendingConstructionOrders.FirstOrDefault()?.Worker?.GetID();
    public ConstructionOrder? OrderOfWorker(IMyUnit worker) => PendingConstructionOrders.FirstOrDefault(o => o.Worker?.GetID() == worker.GetID());

    public ConstructionOrder? OrderOfAddonType(UnitType unitType) => PendingConstructionOrders.FirstOrDefault(o => o.BuildingType == unitType && o.ParentUnit != null);

    public ConstructionOrder RemoveAddonConstructionOrder(UnitType buildingType)
    {
        var order = OrderOfAddonType(buildingType);
        if (order == null)
            throw new ArgumentException($"No construction order of type {buildingType}");

        // RemoveAll works because a worker can only be assigned to one construction at a time
        PendingConstructionOrders.RemoveAll(o => o == order);
        return order;
    }
}