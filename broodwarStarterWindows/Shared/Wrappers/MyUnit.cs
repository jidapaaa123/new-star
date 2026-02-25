using BWAPI.NET;
using Shared.DataAdapters;
using Shared.Interfaces;

namespace Shared.Wrappers
{
    public class MyUnit : IMyUnit, IUnitData
    {
        // Static dictionary to persist scouting state across adapter instances
        private static readonly Dictionary<int, bool> _scoutingState = new();
        private static readonly Dictionary<int, bool> _constructionManagerState = new();


        // The real BWAPI Unit hidden inside
        public Unit Unit { get; }
        public IUnitData UnitData { get; private set; }

        public MyUnit(Unit unit) 
        {
            Unit = unit;
            UnitData = new UnitData(unit);
        } 

        public MyUnit(IUnitData unitData)
        {
            Unit = unitData.Unit;
            UnitData = unitData;
        }

        public bool IsIdle() => Unit.IsIdle();

        public UnitType GetUnitType() => UnitData.GetUnitType();

        public double GetDistance(IMyUnit target)
        {
            // We have to get the real unit out of the target adapter to do the math
            return Unit.GetDistance(target.Unit);
        }

        public int GetDistance(Position target)
        {
               return UnitData.GetDistance(target);
        }

        public bool Move(Position target, bool shiftQueueCommand = false)
        {
            return UnitData.Move(target, shiftQueueCommand);
        }

        public void Gather(IMyUnit target)
        {
            Unit.Gather(target.Unit);
        }

        /// <summary>
        /// Does not call Gather() if it's in PendingConstructionOrder, already carrying any material, or gathering the same resource type.
        /// But will call Gather() if it's gathering the wrong resource type.
        /// </summary>
        public bool SmartGather(IConstructionManager constructionManager, IMyUnit resource)
        {
            IMyUnit currentTarget = GetOrderTarget();
            bool isTargetingCorrectResource = currentTarget != null && (currentTarget.GetID() == resource.GetID() || currentTarget.GetUnitType() == resource.GetUnitType());

            if (IsCarryingMaterial() || isTargetingCorrectResource || IsGatheringGas() || IsInConstructionManager() || IsConstructing())
            {
                return false;
            }

            Gather(resource);
            return true;
        }

        public bool IsGatheringGas()
        {
            return UnitData.IsGatheringGas();
        }

        public Position GetPosition()
        {
            return Unit.GetPosition();
        }

        public bool IsConstructing()
        {
            return UnitData.IsConstructing();
        }

        public bool IsGatheringMinerals()
        {
            return Unit.IsGatheringMinerals();
        }

        public bool IsCarryingMaterial()
        {
            return UnitData.IsCarryingMinerals() || UnitData.IsCarryingGas();
        }

        public bool IsGatheringMaterial()
        {
            return Unit.IsGatheringMinerals() || Unit.IsGatheringGas();
        }

        public bool Build(UnitType buildingType, TilePosition tilePosition)
        {
            return UnitData.Build(buildingType, tilePosition);
        }

        public List<UnitType> GetTrainingQueue()
        {
            return Unit.GetTrainingQueue();
        }

        public bool IsTraining()
        {
            return Unit.IsTraining();
        }

        public bool Train(UnitType type)
        {
            return Unit.Train(type);
        }

        public void RightClick(IMyUnit target)
        {
            Unit.RightClick(target.Unit);
        }

        public int MineralPrice()
        {
            return Unit.GetUnitType().MineralPrice();
        }

        public int GasPrice()
        {
            return Unit.GetUnitType().GasPrice();
        }

        public bool BuildAddon(UnitType addonType)
        {
            return Unit.BuildAddon(addonType);
        }

        public int GetID()
        {
            return UnitData.GetID();
        }

        public IMyUnit? GetOrderTarget()
        {
            var target = Unit.GetOrderTarget();
            return target is null ? null : new MyUnit(target);
        }

        public IMyUnit? GetAddon()
        {
            var addon = Unit.GetAddon();
            return addon is null ? null : new MyUnit(addon);
        }

        public TilePosition GetTilePosition()
        {
            return Unit.GetTilePosition();
        }

        public bool ReturnCargo()
        {
            return Unit.ReturnCargo();
        }

        public bool Gather(IMyUnit target, bool shiftQueueCommand)
        {
            return Unit.Gather(target.Unit, shiftQueueCommand);
        }

        public Order GetOrder()
        {
            return Unit.GetOrder();
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

        public bool IsSelected() => UnitData.IsSelected();

        public static MyUnit CreateForTesting(IUnitData unitData)
        {
            return new MyUnit(unitData);
        }
        public static MyUnit CreateForTesting(IMyUnit unit)
        {
            return new MyUnit(unit.Unit);
        }

        public void SetConstructionManagerStatus(bool v)
        {
            _constructionManagerState[GetID()] = v;
        }

        public bool IsInConstructionManager() => _constructionManagerState.TryGetValue(GetID(), out bool isInManager) ? isInManager : false;

        public bool IsCarryingGas() => UnitData.IsCarryingGas();

        public bool IsCarryingMinerals() => UnitData.IsCarryingMinerals();

        public void Research(TechType type) => UnitData.Research(type);

        public bool IsCompleted() => UnitData.IsCompleted();

        public bool HasPath(Position position) => UnitData.HasPath(position);
    }
}
