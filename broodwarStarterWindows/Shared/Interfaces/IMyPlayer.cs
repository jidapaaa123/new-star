using BWAPI.NET;
using Shared.Wrappers;

namespace Shared.Interfaces
{
    public interface IMyPlayer
    {
        IPlayerData PlayerData { get; }
        IEnumerable<IMyUnit> GetUnits();
        int TotalUnitsIncludingInQueue(UnitType type);
        int GetSupplyUsed();
        int SupplyTotal();
        int Minerals();
        int Gas();
        List<IMyUnit> GetBases();
        List<IMyUnit> GetWorkerUnits();
        bool EnoughAvailableMaterialsToBuild(UnitType unitType, IConstructionManager constructionManager);
        bool EnoughAvailableMaterialsToResearch(TechType type, IConstructionManager constructionManager);
        bool SendTheseWorkersToGatherAt(IConstructionManager constructionManager, List<IMyUnit> availableWorkers, IMyUnit? at);
        TilePosition GetStartLocation();
        bool CanResearch(TechType type, out IMyUnit? productionBuilding);
        bool TryResearch(TechType type, IConstructionManager constructionManager);
        bool HasIncompleteUnitOfType(UnitType unitType);
    }
}
