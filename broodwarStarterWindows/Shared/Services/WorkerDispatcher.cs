using BWAPI.NET;
using Shared.Interfaces;
using Shared.Models;

namespace Shared.Services
{
    public class WorkerDispatcher
    {
        /// <summary>
        /// Orders idle workers to gather materials - distributes them between minerals and gas.
        /// </summary>
        public void OrderIdleUnitsToGatherMaterials(
            IMyGame game,
            IConstructionManager constructionManager,
            GameStrategy strategy)
        {
            IMyPlayer player = game.Self();
            if (player == null || game == null || constructionManager == null)
                return;

            var bases = player.GetBases();
            if (bases.Count == 0)
                return;

            var nearestMineral = game.ClosestInstanceOfTo(game.GetMinerals(), bases[0]);
            if (nearestMineral == null)
                return;

            var availableWorkers = player.GetWorkerUnits()
                .Where(u => StaticGameInfo.IsAvailable(u))
                .ToList();
            if (availableWorkers.Count == 0)
                return;

            var refinery = player.GetUnits()
                .FirstOrDefault(u => u.GetUnitType().IsRefinery() && !u.IsConstructing());

            int actualGasConfig = refinery == null ? 0 : strategy.GasGatherConfig;
            (int mineralWorkersNeeded, int gasWorkersNeeded) = StaticGameInfo.DistributeWorkersForGathering(
                availableWorkers.Count,
                actualGasConfig,
                strategy.MinimumMineralGatherConfig);

            List<IMyUnit> gasWorkers = availableWorkers.GetRange(0, gasWorkersNeeded);
            List<IMyUnit> mineralWorkers = availableWorkers.GetRange(gasWorkersNeeded, mineralWorkersNeeded);

            player.SendTheseWorkersToGatherAt(constructionManager, gasWorkers, refinery);
            player.SendTheseWorkersToGatherAt(constructionManager, mineralWorkers, nearestMineral);
        }

    }
}
