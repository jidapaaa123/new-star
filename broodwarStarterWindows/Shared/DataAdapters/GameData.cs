using BWAPI.NET;
using Shared.Interfaces;

namespace Shared.DataAdapters
{
    public class GameData : IGameData
    {
        public Game Game { get; private set; }

        public GameData(Game game) => Game = game;

        public TilePosition GetBuildLocation(UnitType targetType, TilePosition desiredPosition, int maxRange)
            => Game.GetBuildLocation(targetType, desiredPosition, maxRange);

        public List<IUnitData> GetMinerals()
            => Game.GetMinerals().Select(m => (IUnitData)new UnitData(m)).ToList();

        public IPlayerData Self()
            => new PlayerData(Game.Self());

        public void SendText(string text)
            => Game.SendText(text);

        public List<IUnitData> GetNeutralUnits()
        {
            return Game.GetNeutralUnits()
                .Select(u => (IUnitData)new UnitData(u))
                .ToList();
        }

        public IEnumerable<TilePosition> GetStartLocations()
        {
            return Game.GetStartLocations();
        }

        public IPlayerData Enemy() => new PlayerData(Game.Enemy());

        public bool IsExplored(TilePosition tilePosition) => Game.IsExplored(tilePosition);
    }
}
