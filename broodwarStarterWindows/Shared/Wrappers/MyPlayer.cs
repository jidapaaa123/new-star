using BWAPI.NET;
using Shared.Interfaces;
using Shared.Models;
using System.Numerics;

namespace Shared.Wrappers
{
    public class MyPlayer : IMyPlayer
    {
        public IPlayerData PlayerData { get; private set; }

        public MyPlayer(IPlayerData playerData) 
        {
            PlayerData = playerData;
        } 

        public Materials GetAvailableMaterials(IConstructionManager constructionManager)
        {
            (int reservedMinerals, int reservedGas) = constructionManager.GetReservedMaterials();
            return new Materials
            {
                Minerals = Minerals() - reservedMinerals,
                Gas = Gas() - reservedGas
            };
        }

        public int TotalUnitsIncludingInQueue(UnitType type)
        {
            int currentCount = PlayerData.GetUnits().Count(u => u.GetUnitType() == type);
            IUnitData? productionBuilding = PlayerData.GetUnits().FirstOrDefault(u => u.GetUnitType() == UnitType.Terran_Command_Center);
            int unitsInQueueCount = productionBuilding?.GetTrainingQueue().Count(u => u == type) ?? 0;
            return currentCount + unitsInQueueCount;
        }

        public bool EnoughAvailableMaterialsToBuild(UnitType unitType, IConstructionManager constructionManager)
        {
            (int availableMinerals, int availableGas) = GetAvailableMaterials(constructionManager);
            return availableMinerals >= unitType.MineralPrice() && availableGas >= unitType.GasPrice();
        }

        public bool EnoughAvailableMaterialsToResearch(TechType type, IConstructionManager constructionManager)
        {
            (int availableMinerals, int availableGas) = GetAvailableMaterials(constructionManager);
            return availableMinerals >= type.MineralPrice() && availableGas >= type.GasPrice();
        }

        public bool HasPrerequisitesForBuilding(UnitType buildingType)
        {
            var prerequisites = buildingType.RequiredUnits();
            return prerequisites.All(prereq =>
                    PlayerData.CompletedUnitCount(prereq.Key) > prereq.Value / 2); // refer to Tests to why this offset is done
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

            var parentType = StaticGameInfo.GetAddonParentType(buildingType);
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
                    .FirstOrDefault(w => StaticGameInfo.IsAvailable(w));
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
                    Where(u => StaticGameInfo.TerranBaseType().Contains(u.GetUnitType()))
                   .ToList();
        }

        public int Gas()
        {
            return PlayerData.Gas();
        }

        public int GetSupplyUsed()
        {
            // BWAPI's SupplyUsed counts in half-units, so we divide by 2 to get the standard unit count
            return PlayerData.SupplyUsed() / 2;
        }

        public int SupplyTotal()
        {
            return PlayerData.SupplyTotal() / 2;
        }

        public IEnumerable<IMyUnit> GetUnits()
        {
            // Convert IUnit to IMyUnit - if it's a UnitAdapter, it implements both
            return PlayerData.GetUnits().OfType<IMyUnit>();
        }

        public int Minerals()
        {
            return PlayerData.Minerals();
        }

        public bool SendTheseWorkersToGatherAt(IConstructionManager constructionManager, List<IMyUnit> availableWorkers, IMyUnit? at)
        {
            if (at is null)
                return false;
            foreach (var worker in availableWorkers)
            {
                bool success = worker.SmartGather(constructionManager, at);
            }

            return true;
        }

        public static MyPlayer CreateForTesting(IPlayerData playerData)
        {
            return new MyPlayer(playerData);
        }

        public TilePosition GetStartLocation() => PlayerData.GetStartLocation();

        public bool CanResearch(TechType type, out IMyUnit? productionBuilding)
        {
            UnitType prodUnitType = type.WhatResearches();
            int completedCount = PlayerData.CompletedUnitCount(prodUnitType);
            productionBuilding = 
                GetUnits()
                .Where(u => u.GetUnitType() == prodUnitType && u.IsCompleted())
                .FirstOrDefault();
            if (productionBuilding == null)
                return false;

            bool alreadyResearched = PlayerData.HasResearched(type);
            return !alreadyResearched;
        }

        public bool TryResearch(TechType type, IConstructionManager constructionManager)
        {
            bool sufficientMats = EnoughAvailableMaterialsToResearch(type, constructionManager);
            bool canResearch = CanResearch(type, out IMyUnit? productionBuilding);

            if (!(sufficientMats && canResearch) || productionBuilding is null)
                return false;

            productionBuilding.Research(type);
            return true;
        }

        /// <summary>
        /// Checks if there's an incomplete unit of the specified type currently under construction.
        /// </summary>
        /// <param name="unitType">The unit type to check for (e.g., Terran_Supply_Depot)</param>
        /// <returns>True if a unit of this type is currently being constructed, false otherwise</returns>
        public bool HasIncompleteUnitOfType(UnitType unitType)
        {
            return GetUnits()
                .Where(u => u.GetUnitType() == unitType)
                .Any(u => u.IsConstructing());
        }
    }
}
