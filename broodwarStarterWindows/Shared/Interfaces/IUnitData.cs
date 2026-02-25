using BWAPI.NET;

namespace Shared.Interfaces
{
    public interface IUnitData
    {
        Unit Unit { get; }
        UnitType GetUnitType();
        List<UnitType> GetTrainingQueue();
        bool IsGatheringGas();
        bool IsCarryingGas();
        bool IsCarryingMinerals();
        bool Build(UnitType buildingType, TilePosition tilePosition);
        int GetID();
        bool IsSelected();
        bool IsConstructing();
        int GetDistance(Position position);
        bool Move(Position target, bool shiftQueueCommand);
        void Research(TechType type);
        bool IsCompleted();
        bool HasPath(Position position);
    }
}
