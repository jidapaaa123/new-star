using BWAPI.NET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Interfaces
{
    public interface IMyUnit
    {
        bool IsIdle();
        void Gather(IMyUnit target);
        bool Gather(IMyUnit target, bool shiftQueueCommand);
        double GetDistance(IMyUnit target);
        int GetDistance(Position target);
        bool Move(Position target, bool shiftQueueCommand = false);
        Position GetPosition();
        bool IsConstructing();
        bool IsGatheringMinerals();
        bool IsGatheringGas();
        bool IsCarryingMaterial();
        bool IsGatheringMaterial();
        bool SmartGather(IConstructionManager constructionManager, IMyUnit resource);
        bool Build(UnitType buildingType, TilePosition position);
        BWAPI.NET.UnitType GetUnitType(); // Keep the real UnitType here
        List<UnitType> GetTrainingQueue();
        bool IsTraining();
        bool Train(UnitType terran_SCV);
        void RightClick(IMyUnit nearestDeposit);
        int MineralPrice();
        int GasPrice();
        bool BuildAddon(UnitType addonType);
        int GetID();
        IMyUnit GetOrderTarget();
        Order GetOrder();
        IMyUnit GetAddon();
        TilePosition GetTilePosition();
        bool ReturnCargo();
        void SetScouting();
        void UnsetScouting();
        bool IsScouting();

        BWAPI.NET.Unit UnderlyingUnit { get; }
    }
}
