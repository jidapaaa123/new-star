using BWAPI.NET;
using Shared.Interfaces;
using Shared.Wrappers;

namespace Shared.Models
{
    public class ScoutingManager : IScoutingManager
    {
        public MyGame Game { get; private set; }

        private IMapManager? _mapManager;

        public List<ScoutLocation> PotentialBases { get; private set; } = new();
        public bool IsScoutingEnabled { get; private set; } = false;
        public IMyUnit? ScoutUnit { get; private set; } = null;
        private int _scoutTargetIndex = 0;

        public ScoutingManager(MyGame game, IMapManager? mapManager)
        {
            this.Game = game;
            _mapManager = mapManager;
            PotentialBases = _mapManager?.GetScoutingTargets()
                ?.Select(tp => new ScoutLocation(tp))
                .ToList() ?? new List<ScoutLocation>();
        }

        public void UpdateScouting(bool isScoutingEnabled)
        {
            // set up cycle
            if (isScoutingEnabled && (isScoutingEnabled != IsScoutingEnabled || PotentialBases.Count == 0))
            {
                resetPotentialBases();
                IsScoutingEnabled = true;
                tryEnsureScoutUnitIsSet();
            }
            // tear down cycle
            else if (!isScoutingEnabled && isScoutingEnabled != IsScoutingEnabled)
            {
                if (ScoutUnit != null)
                {
                    ScoutUnit.UnsetScouting();
                    ScoutUnit = null;
                }
                IsScoutingEnabled = false;
            }

            if (IsScoutingEnabled)
            {
                progressScoutCycle();
            }
        }

        private void resetPotentialBases()
        {
            PotentialBases = PotentialBases
                .Select(loc => new ScoutLocation(loc.TilePosition)
                    {
                        IsExplored = false,
                        EnemyFound = loc.EnemyFound
                })
                .ToList();
            _scoutTargetIndex = 0;
        }

        private bool tryEnsureScoutUnitIsSet()
        {
            if (ScoutUnit == null)
            {
                var player = Game.Self();
                ScoutUnit = player.GetWorkerUnits().FirstOrDefault(u => StaticGameInfo.IsAvailable(u));
                ScoutUnit?.SetScouting();
            }

            return ScoutUnit != null;
        }

        private void proceedToNextScoutingLocation()
        {
            if (ScoutUnit == null)
                return;

            int nextIndex = PotentialBases.FindIndex(s => !s.IsExplored);
            bool notFound = nextIndex == -1;

            if (notFound)
            {
                Position home = Game.Self().GetStartLocation().ToPosition();
                ScoutUnit.Move(home);
                ScoutUnit.UnsetScouting();
                ScoutUnit = null;
                IsScoutingEnabled = false;
            }
            else
            {
                _scoutTargetIndex = nextIndex;
            }
        }

        public void progressScoutCycle()
        {
            if (!tryEnsureScoutUnitIsSet())
                return;

            Position targetPosition = PotentialBases[_scoutTargetIndex].TilePosition.ToPosition();
            bool reachedTarget = ScoutUnit.GetDistance(targetPosition) <= 200;

            // Check for enemies near target EVERY FRAME, regardless of scout position
            if (IsEnemyNearTargetLocation(targetPosition))
            {
                // Found enemy close enough to target location - mark as found and proceed
                PotentialBases[_scoutTargetIndex].IsExplored = true;
                PotentialBases[_scoutTargetIndex].EnemyFound = true;

                Console.WriteLine($"Scout found enemy at base {_scoutTargetIndex}. Total bases: {PotentialBases.Count}. Unexplored: {PotentialBases.Count(b => !b.IsExplored)}");
                proceedToNextScoutingLocation();
                Console.WriteLine($"After proceeding, scout target index: {_scoutTargetIndex}");
                return;
            }

            // No enemy near target - check if we've reached the location
            if (reachedTarget)
            {
                // Reached target location and confirmed no enemy nearby - mark as explored
                PotentialBases[_scoutTargetIndex].IsExplored = true;
                PotentialBases[_scoutTargetIndex].EnemyFound = false;

                Console.WriteLine($"Scout reached base {_scoutTargetIndex} with no enemy. Total bases: {PotentialBases.Count}. Unexplored: {PotentialBases.Count(b => !b.IsExplored)}");
                proceedToNextScoutingLocation();
                Console.WriteLine($"After proceeding, scout target index: {_scoutTargetIndex}");
            }
            else
            {
                // Haven't reached target yet and no enemy detected - keep marching toward it
                ScoutUnit.Move(targetPosition);
            }
        }

        /// <summary>
        /// Checks if any enemy units are within a reasonable distance of the target location.
        /// Uses 800 pixel radius - large enough to catch enemies deep in the base,
        /// but small enough to avoid flagging enemies in nearby bases.
        /// </summary>
        private bool IsEnemyNearTargetLocation(Position targetPosition, int searchRadius = 800)
        {
            if (!Game.Enemy().GetUnits().Any())
                return false;

            return Game.Enemy().GetUnits()
                .Any(u => u.GetDistance(targetPosition) <= searchRadius);
        }


    }
}
