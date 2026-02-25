using BWAPI.NET;

namespace Shared.Interfaces
{
    public interface IPlayerData
    {
        List<IUnitData> GetUnits();
        int Gas();
        int SupplyUsed();
        int SupplyTotal();
        int Minerals();
        int CompletedUnitCount(UnitType unitType);
        TilePosition GetStartLocation();
        bool HasResearched(TechType type);
    }
}