using BWAPI.NET;
using Shared.Interfaces;

namespace Shared.DataAdapters
{
    public class UnitData : IUnitData
    {
        private readonly Unit _unit;
        public Unit Unit => _unit;
        public UnitData(Unit unit) => _unit = unit;

        public UnitType GetUnitType() => _unit.GetUnitType();
        public List<UnitType> GetTrainingQueue() => _unit.GetTrainingQueue();

        public bool IsGatheringGas() => Unit.IsGatheringGas();
        public bool IsCarryingGas() => Unit.IsCarryingGas();
        public bool IsCarryingMinerals() => Unit.IsCarryingMinerals();

        public bool Build(UnitType buildingType, TilePosition tilePosition) => Unit.Build(buildingType, tilePosition);

        public int GetID() => Unit.GetID();

        public bool IsSelected() => Unit.IsSelected();

        public bool IsConstructing() => Unit.IsConstructing();
        public bool IsGatheringMinerals() => Unit.IsGatheringMinerals();
        public bool IsCarryingMaterial() => Unit.IsCarryingMinerals() || Unit.IsCarryingGas();
        public bool IsGatheringMaterial() => Unit.IsGatheringMinerals() || Unit.IsGatheringGas();
        public bool Train(UnitType type) => Unit.Train(type);
        public bool IsTraining() => Unit.IsTraining();
        public void RightClick(IMyUnit target) => Unit.RightClick(target.Unit);
        public int MineralPrice() => Unit.GetUnitType().MineralPrice();
        public int GasPrice() => Unit.GetUnitType().GasPrice();
        public bool BuildAddon(UnitType addonType) => Unit.BuildAddon(addonType);
        public int GetDistance(Position position) => Unit.GetDistance(position);

        public bool Move(Position target, bool shiftQueueCommand) => Unit.Move(target, shiftQueueCommand);

        public void Research(TechType type) => Unit.Research(type);

        public bool IsCompleted() => Unit.IsCompleted();

        public bool HasPath(Position position) => Unit.HasPath(position);
    }
}
