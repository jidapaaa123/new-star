using BWAPI.NET;
using Shared.DataAdapters;
using Shared.Interfaces;

namespace Shared.Wrappers
{
    public class MyGame : IMyGame
    {
        public IGameData GameData { get; private set; }
        public MyGame(IGameData gameData)
        {
            GameData = gameData;
        }

        public TilePosition GetBuildLocation(UnitType targetType, TilePosition desiredPosition, int maxRange)
        {
            return GameData.GetBuildLocation(targetType, desiredPosition, maxRange);
        }

        public List<IMyUnit> GetMinerals()
        {
            // IGameData.GetMinerals() already returns List<IUnitData> (wrapped in UnitDataAdapter)
            // We convert IUnitData to MyUnit and cast to IMyUnit
            return GameData.GetMinerals()
                              .Select(m => (IMyUnit)new MyUnit(m))
                              .ToList();
        }

        public IMyPlayer Self()
        {
            return new MyPlayer(GameData.Self());
        }

        public void SendText(string text)
        {
            GameData.SendText(text);
        }

        public IMyUnit? UnitOfTypeNearestTo(UnitType type, IMyUnit to)
        {
            var unit = GameData.Self()
                .GetUnits()
                .Where(u => u.GetUnitType() == type)
                .Select(u => new MyUnit(u))
                .OrderBy(u => u.GetDistance(to))
                .FirstOrDefault();

            return unit;
        }

        public IMyUnit? ClosestInstanceOfTo(List<IMyUnit> instances, IMyUnit to)
        {
            var unit = instances
                .OrderBy(u => u.GetDistance(to))
                .FirstOrDefault();

            return unit;
        }

        public static MyGame CreateForTesting(IGameData gameData)
        {
            return new MyGame(gameData);
        }

        public IPlayerData Enemy() => GameData.Enemy();
    }
}
