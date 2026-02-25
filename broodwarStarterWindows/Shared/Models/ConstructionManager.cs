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
    public ConstructionOrder? PendingConstructionOrder { get; private set; }

    /// <summary>
    /// Adds to PendingConstructionOrders and commands the worker to build.
    /// </summary>
    /// <param name="type"></param>
    /// <param name="worker"></param>
    /// <param name="tilePosition"></param>
    public void RegisterOrder(UnitType type, IMyUnit worker, TilePosition tilePosition, bool isFromBuildOrder)
    {
        var order = new ConstructionOrder
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
        };

        RegisterOrder(order);
    }

    public void RegisterOrder(ConstructionOrder order)
    {
        IMyUnit? worker = order.Worker;
        var type = order.BuildingType;
        var tilePosition = order.TilePosition;

        if (worker.IsCarryingMaterial())
        {
            worker.ReturnCargo();
        }

        PendingConstructionOrder = order;
        worker.Build(type, tilePosition);
        worker.SetConstructionManagerStatus(true);
    }

    /// <summary>
    /// Calls the PendingConstructionOrder's worker to build it
    /// </summary>
    public void RecalibrateWorker(IGameData gameData)
    {
        if (PendingConstructionOrder is null)
            return;

        var type = PendingConstructionOrder.BuildingType;
        var worker = PendingConstructionOrder.Worker;
        var tilePosition = PendingConstructionOrder.TilePosition;

        if (!gameData.IsExplored(tilePosition))
        {
            worker.Move(new Position(tilePosition));
            return;
        }

        bool buildSuccess = worker.Build(type, tilePosition);
        if (!buildSuccess)
        {
            // Log why it failed
            System.Diagnostics.Debug.WriteLine(
                $"Build failed: Worker.IsIdle={worker.IsIdle()}, " +
                $"Pos={worker.GetPosition()}, " +
                $"TargetTile={tilePosition}, " +
                $"HasVision={worker.HasPath(new Position(tilePosition))}");
        }
        worker.SetConstructionManagerStatus(true);
    }

    public ConstructionOrder RemovePendingConstructionOrder()
    {
        var order = PendingConstructionOrder;
        if (order == null)
            throw new ArgumentException($"No construction order in ConstructionManager");
        var worker = order.Worker;

        PendingConstructionOrder = null;
        worker.SetConstructionManagerStatus(false);
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

    public int GetReservedMinerals() => PendingConstructionOrder?.Costs.Minerals ?? 0;
    public int GetReservedGas() => PendingConstructionOrder?.Costs.Gas ?? 0;
    public Materials GetReservedMaterials() => new Materials
    {
        Minerals = GetReservedMinerals(),
        Gas = GetReservedGas()
    };
}