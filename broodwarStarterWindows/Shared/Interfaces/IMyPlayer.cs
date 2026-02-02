using BWAPI.NET;

namespace Shared.Interfaces
{
    public interface IMyPlayer
    {
        IEnumerable<IMyUnit> GetUnits();
        int GetSupplyUsed();
        int SupplyTotal();
        int Minerals();
        int Gas();
        List<IMyUnit> GetBases();
        List<IMyUnit> GetWorkerUnits();
        bool EnoughAvailableMaterialsToBuild(UnitType unitType, IConstructionManager constructionManager);
    }
}
