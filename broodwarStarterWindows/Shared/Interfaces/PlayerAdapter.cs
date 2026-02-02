using BWAPI.NET;
using Shared.Models;
using Shared.MyLogic;
using System.Numerics;

namespace Shared.Interfaces
{
    public class PlayerAdapter : IMyPlayer
    {
        private readonly Player _player;
        public PlayerAdapter(Player player) => _player = player;

        public Materials GetAvailableMaterials(IConstructionManager constructionManager)
        {
            (int reservedMinerals, int reservedGas) = constructionManager.GetReservedMaterials();
            return new Materials
            {
                Minerals = Minerals() - reservedMinerals,
                Gas = Gas() - reservedGas
            };
        }

        public bool EnoughAvailableMaterialsToBuild(UnitType unitType, IConstructionManager constructionManager)
        {
            (int availableMinerals, int availableGas) = GetAvailableMaterials(constructionManager);
            return availableMinerals >= unitType.MineralPrice() && availableGas >= unitType.GasPrice();
        }

        public bool HasPrerequisitesForBuilding(UnitType buildingType)
        {
            var prerequisites = buildingType.RequiredUnits();
            return prerequisites.All(prereq =>
                    _player.CompletedUnitCount(prereq.Key) > prereq.Value / 2); // refer to Tests to why this offset is done
        }

        /// <summary>
        /// Finds an available worker & if supplies available, 
        /// registers a Construction Order for the specified building type at the given tile position.
        /// </summary>
        /// <param name="constructionManager"></param>
        /// <param name="buildingType"></param>
        /// <param name="tilePosition"></param>
        /// <returns>Whether a Construction Order was actually registered</returns>
        public bool TryConstruct(IConstructionManager constructionManager, UnitType buildingType, TilePosition tilePosition, bool isFromBuildOrder)
        {
            bool sufficientMats = EnoughAvailableMaterialsToBuild(buildingType, constructionManager);
            bool hasPrereqs = HasPrerequisitesForBuilding(buildingType);
            if (!sufficientMats || !hasPrereqs)
            {
                return false;
            }

            var parentType = HelperLogic.GetAddonParentType(buildingType);
            bool isAddon = parentType != UnitType.None;

            if (isAddon)
            {
                var parentUnits = GetUnits()
                    .Where(u => u.GetUnitType() == parentType && u.GetAddon() == null)
                    .ToList();
                if (parentUnits.Count == 0)
                    return false;

                var parentUnit = parentUnits.First();
                return parentUnit.BuildAddon(buildingType);
            }
            else
            {
                IMyUnit? availableWorker = GetWorkerUnits()
                    .FirstOrDefault(w => HelperLogic.IsAvailable(constructionManager, w));
                if (availableWorker != null)
                {
                    constructionManager.RegisterOrder(buildingType, availableWorker, tilePosition, isFromBuildOrder);
                    return true;
                }
            }

            return false;
        }

        public List<IMyUnit> GetWorkerUnits()
        {
            return GetUnits().Where(u => u.GetUnitType().IsWorker()).ToList();
        }

        public List<IMyUnit> GetBases()
        {
            return GetUnits().
                    Where(u => HelperLogic.TerranBaseType().Contains(u.GetUnitType()))
                   .ToList();
        }

        public int Gas()
        {
            return _player.Gas();
        }

        public int GetSupplyUsed()
        {
            // BWAPI's SupplyUsed counts in half-units, so we divide by 2 to get the standard unit count
            return _player.SupplyUsed() / 2;
        }

        public int SupplyTotal()
        {
            return _player.SupplyTotal() / 2;
        }

        public IEnumerable<IMyUnit> GetUnits()
        {
            // Wrap every real unit in an adapter as we return it
            return _player.GetUnits().Select(u => new UnitAdapter(u));
        }

        public int Minerals()
        {
            return _player.Minerals();
        }

        public bool SendTheseWorkersToGatherAt(IConstructionManager constructionManager, List<IMyUnit> availableWorkers, IMyUnit at)
        {
            if (at is null)
                return false;
            foreach (var worker in availableWorkers)
            {
                bool success = worker.SmartGather(constructionManager, at);
            }

            return true;
        }
    }
}
