using BWAPI.NET;

namespace Shared.Interfaces
{
    public class GameAdapter : IMyGame
    {
        private readonly Game _actualGame;

        public GameAdapter(Game game)
        {
            _actualGame = game;
        }

        public TilePosition GetBuildLocation(UnitType targetType, TilePosition desiredPosition, int maxRange)
        {
            return _actualGame.GetBuildLocation(targetType, desiredPosition, maxRange);
        }

        public List<IMyUnit> GetMinerals()
        {
            // Convert the list of real Units into a list of UnitAdapters
            return _actualGame.GetMinerals()
                              .Select(m => new UnitAdapter(m))
                              .Cast<IMyUnit>()
                              .ToList();
        }

        public IMyPlayer Self()
        {
            return new PlayerAdapter(_actualGame.Self());
        }

        public void SendText(string text)
        {
            _actualGame.SendText(text);
        }

        public IMyUnit? UnitOfTypeNearestTo(UnitType type, IMyUnit to)
        {
            var unit = _actualGame.Self()
                .GetUnits()
                .Where(u => u.GetUnitType() == type)
                .Select(u => new UnitAdapter(u))
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
    }
}
