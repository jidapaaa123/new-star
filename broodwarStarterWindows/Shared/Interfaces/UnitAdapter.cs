using BWAPI.NET;

namespace Shared.Interfaces
{
    public class UnitAdapter : IMyUnit
    {
        // Static dictionary to persist scouting state across adapter instances
        private static readonly Dictionary<int, bool> _scoutingState = new();

        // The real BWAPI Unit hidden inside
        public Unit UnderlyingUnit { get; }

        public UnitAdapter(Unit unit) => UnderlyingUnit = unit;

        public bool IsIdle() => UnderlyingUnit.IsIdle();

        public UnitType GetUnitType() => UnderlyingUnit.GetUnitType();

        public double GetDistance(IMyUnit target)
        {
            // We have to get the real unit out of the target adapter to do the math
            return UnderlyingUnit.GetDistance(target.UnderlyingUnit);
        }

        public int GetDistance(Position target)
        {
               return UnderlyingUnit.GetDistance(target);
        }

        public bool Move(Position target, bool shiftQueueCommand = false)
        {
            return UnderlyingUnit.Move(target, shiftQueueCommand);
        }

        public void Gather(IMyUnit target)
        {
            UnderlyingUnit.Gather(target.UnderlyingUnit);
        }

        /// <summary>
        /// Does not call Gather() if it's in PendingConstructionOrder, already carrying any material, or gathering the same resource type.
        /// But will call Gather() if it's gathering the wrong resource type.
        /// </summary>
        public bool SmartGather(IConstructionManager constructionManager, IMyUnit resource)
        {
            IMyUnit currentTarget = GetOrderTarget();
            bool isTargetingCorrectResource = currentTarget != null && (currentTarget.GetID() == resource.GetID() || currentTarget.GetUnitType() == resource.GetUnitType());

            if (IsCarryingMaterial() || isTargetingCorrectResource || IsGatheringGas() || constructionManager.IsWorkerAssignedToConstruction(this) || IsConstructing())
            {
                return false;
            }

            Gather(resource);
            return true;
        }

        public bool IsGatheringGas()
        {
            return UnderlyingUnit.IsGatheringGas();
        }

        public Position GetPosition()
        {
            return UnderlyingUnit.GetPosition();
        }

        public bool IsConstructing()
        {
            return UnderlyingUnit.IsConstructing();
        }

        public bool IsGatheringMinerals()
        {
            return UnderlyingUnit.IsGatheringMinerals();
        }

        public bool IsCarryingMaterial()
        {
            return UnderlyingUnit.IsCarryingMinerals() || UnderlyingUnit.IsCarryingGas();
        }

        public bool IsGatheringMaterial()
        {
            return UnderlyingUnit.IsGatheringMinerals() || UnderlyingUnit.IsGatheringGas();
        }

        public bool Build(UnitType buildingType, TilePosition tilePosition)
        {
            return UnderlyingUnit.Build(buildingType, tilePosition);
        }

        public List<UnitType> GetTrainingQueue()
        {
            return UnderlyingUnit.GetTrainingQueue();
        }

        public bool IsTraining()
        {
            return UnderlyingUnit.IsTraining();
        }

        public bool Train(UnitType type)
        {
            return UnderlyingUnit.Train(type);
        }

        public void RightClick(IMyUnit target)
        {
            UnderlyingUnit.RightClick(target.UnderlyingUnit);
        }

        public int MineralPrice()
        {
            return UnderlyingUnit.GetUnitType().MineralPrice();
        }

        public int GasPrice()
        {
            return UnderlyingUnit.GetUnitType().GasPrice();
        }

        public bool BuildAddon(UnitType addonType)
        {
            return UnderlyingUnit.BuildAddon(addonType);
        }

        public int GetID()
        {
            return UnderlyingUnit.GetID();
        }

        public IMyUnit? GetOrderTarget()
        {
            var target = UnderlyingUnit.GetOrderTarget();
            return target is null ? null : new UnitAdapter(target);
        }

        public IMyUnit? GetAddon()
        {
            var addon = UnderlyingUnit.GetAddon();
            return addon is null ? null : new UnitAdapter(addon);
        }

        public TilePosition GetTilePosition()
        {
            return UnderlyingUnit.GetTilePosition();
        }

        public bool ReturnCargo()
        {
            return UnderlyingUnit.ReturnCargo();
        }

        public bool Gather(IMyUnit target, bool shiftQueueCommand)
        {
            return UnderlyingUnit.Gather(target.UnderlyingUnit, shiftQueueCommand);
        }

        public Order GetOrder()
        {
            return UnderlyingUnit.GetOrder();
        }

        public void SetScouting()
        {
            _scoutingState[GetID()] = true;
        }

        public void UnsetScouting()
        {
            _scoutingState[GetID()] = false;
        }

        public bool IsScouting() => _scoutingState.TryGetValue(GetID(), out bool isScouting) ? isScouting : false;
    }
}
