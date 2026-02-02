using BWAPI.NET;

namespace Shared.Interfaces
{
    public interface IMyGame
    {
        List<IMyUnit> GetMinerals();
        TilePosition GetBuildLocation(UnitType targetType, TilePosition desiredPosition, int maxRange);
        void SendText(string text);
        IMyPlayer Self();
    }
}
