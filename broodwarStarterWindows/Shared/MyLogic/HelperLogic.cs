using BWAPI.NET;
using Shared.Interfaces;

namespace Shared.MyLogic
{
    public class HelperLogic
    {
        public const string ApiBaseAddress = "https://localhost:7138/api/bot/";
        public const int GasGatherConfigDefault = 3;

        /// <summary>
        /// Recognizes non-idle workers who are just defaulted to Gathering
        /// </summary>
        /// <returns></returns>
        public static bool IsAvailable(IConstructionManager constructionManager, IMyUnit worker)
        {
            // 1. If it's selected by the human, it's NOT available.
            if (worker.UnderlyingUnit.IsSelected()) return false;

            return !(constructionManager.IsWorkerAssignedToConstruction(worker) || worker.IsConstructing() || worker.IsScouting());
        }

        public static UnitType GetAddonParentType(UnitType addonType)
        {
            return addonType switch
            {
                UnitType.Terran_Comsat_Station => UnitType.Terran_Command_Center,
                UnitType.Terran_Nuclear_Silo => UnitType.Terran_Command_Center,
                UnitType.Terran_Control_Tower => UnitType.Terran_Starport,
                UnitType.Protoss_Citadel_of_Adun => UnitType.Protoss_Cybernetics_Core,
                UnitType.Protoss_Forge => UnitType.Protoss_Nexus,
                UnitType.Protoss_Fleet_Beacon => UnitType.Protoss_Stargate,
                UnitType.Zerg_Greater_Spire => UnitType.Zerg_Spire,
                _ => UnitType.None
            };
        }

        public static UnitType[] TerranBaseType()
        {
            UnitType[] baseTypes = new UnitType[]
            {
                UnitType.Terran_Command_Center
            };

            return baseTypes;
        }

        public static TilePosition[] InvalidPositionTypes() => [ TilePosition.Invalid, TilePosition.None, TilePosition.Unknown ];


        public static List<IMyUnit> GetWorkerUnits(IMyGame game, IMyPlayer player)
        {
            List<IMyUnit> workers = new List<IMyUnit>();


            foreach (var unit in player.GetUnits())
            {
                if (unit.GetUnitType().IsWorker())
                {
                    workers.Add(unit);
                }
            }
            return workers;
        }

        public static IMyUnit GetNearestMineralField(IMyGame game, IMyUnit from)
        {
            List<IMyUnit> visibleMineralDeposits = game.GetMinerals();
            IMyUnit? nearestDeposit = null;
            foreach (var deposit in visibleMineralDeposits)
            {
                if (nearestDeposit is null || from.GetDistance(deposit) < from.GetDistance(nearestDeposit))
                {
                    nearestDeposit = deposit;
                }
            }

            return nearestDeposit!;
        }

        public static int TotalSCVIncludingInQueue(IMyGame game, IMyPlayer player)
        {
            int currentSCVCount = player.GetUnits().Count(u => u.GetUnitType() == UnitType.Terran_SCV);
            IMyUnit? commandCenter = player.GetUnits().FirstOrDefault(u => u.GetUnitType() == UnitType.Terran_Command_Center);
            int scvsInQueueCount = commandCenter.GetTrainingQueue().Count(u => u == UnitType.Terran_SCV);
            return currentSCVCount + scvsInQueueCount;
        }
        public static int TotalMarinesIncludingInQueue(IMyGame game, IMyPlayer? player)
        {
            int currentMarineCount = player.GetUnits().Count(u => u.GetUnitType() == UnitType.Terran_Marine);
            IMyUnit? barrack = player.GetUnits().FirstOrDefault(u => u.GetUnitType() == UnitType.Terran_Barracks);
            int marinesInQueueCount = barrack.GetTrainingQueue().Count(u => u == UnitType.Terran_Marine);
            return currentMarineCount + marinesInQueueCount;
        }

        public static int TotalVulturesIncludingInQueue(IMyGame game, IMyPlayer? player)
        {
            int currentVultureCount = player.GetUnits().Count(u => u.GetUnitType() == UnitType.Terran_Vulture);
            IMyUnit? factory = player.GetUnits().FirstOrDefault(u => u.GetUnitType() == UnitType.Terran_Factory);
            int vulturesInQueueCount = factory.GetTrainingQueue().Count(u => u == UnitType.Terran_Vulture);
            return currentVultureCount + vulturesInQueueCount;
        }

        public static void QueueUnits(IMyUnit productionBuilding, UnitType unitType, int count)
        {
            for (int i = 0; i < count; i++)
            {
                productionBuilding.Train(unitType);
            }
        }

    }
}
