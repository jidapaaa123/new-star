using BWAPI.NET;
using Shared.Interfaces;
using Shared.Models;

namespace Shared
{
    public class StaticGameInfo
    {
        public const string ApiBaseAddress = "https://localhost:7138/api/bot/";
        public const int GasGatherConfigDefault = 3;

        /// <summary>
        /// Recognizes non-idle workers who are just defaulted to Gathering
        /// </summary>
        /// <returns></returns>
        public static bool IsAvailable(IMyUnit worker)
        {
            // 1. If it's selected by the human, it's NOT available.
            if (worker.IsSelected()) return false;

            return !(worker.IsInConstructionManager() || worker.IsConstructing() || worker.IsScouting());
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
                UnitType.Terran_Machine_Shop => UnitType.Terran_Factory,
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

        /// <summary>
        /// Returns the default build order with all unit thresholds initialized.
        /// </summary>
        public static List<BuildOrderItem> GetDefaultBuildOrder()
        {
            return new()
            {
                new() { UnitType = UnitType.Terran_Supply_Depot, UnitThreshold = new() { { UnitType.Terran_SCV, 9 }, { UnitType.Terran_Marine, 0 }, { UnitType.Terran_Vulture, 0 } } },
                new() { UnitType = UnitType.Terran_Barracks, UnitThreshold = new() { { UnitType.Terran_SCV, 11 }, { UnitType.Terran_Marine, 0 }, { UnitType.Terran_Vulture, 0 } } },
                new() { UnitType = UnitType.Terran_Refinery, UnitThreshold = new() { { UnitType.Terran_SCV, 13 }, { UnitType.Terran_Marine, 2 }, { UnitType.Terran_Vulture, 0 } } },
                new() { UnitType = UnitType.Terran_Supply_Depot, UnitThreshold = new() { { UnitType.Terran_SCV, 13 }, { UnitType.Terran_Marine, 2 }, { UnitType.Terran_Vulture, 0 } } },
                new() { UnitType = UnitType.Terran_Factory, UnitThreshold = new() { { UnitType.Terran_SCV, 16 }, { UnitType.Terran_Marine, 4 }, { UnitType.Terran_Vulture, 0 } } },
                new() { UnitType = UnitType.Terran_Machine_Shop, UnitThreshold = new() { { UnitType.Terran_SCV, 16 }, { UnitType.Terran_Marine, 6 }, { UnitType.Terran_Vulture, 0 } } },
                new() { TechType = TechType.Tank_Siege_Mode, UnitThreshold = new() { { UnitType.Terran_SCV, 16 }, { UnitType.Terran_Marine, 8 }, { UnitType.Terran_Vulture, 0 } } },
                new() { UnitType = UnitType.Terran_Starport, UnitThreshold = new() { { UnitType.Terran_SCV, 17 }, { UnitType.Terran_Marine, 8 }, { UnitType.Terran_Vulture, 0 } } },
                new() { UnitType = UnitType.Terran_Control_Tower, UnitThreshold = new() { { UnitType.Terran_SCV, 18 }, { UnitType.Terran_Marine, 8 }, { UnitType.Terran_Vulture, 1 } } },
                new() { UnitType = UnitType.Terran_Supply_Depot, UnitThreshold = new() { { UnitType.Terran_SCV, 18 }, { UnitType.Terran_Marine, 8 }, { UnitType.Terran_Vulture, 1 } } },
                new() { TechType = TechType.Cloaking_Field, UnitThreshold = new() { { UnitType.Terran_SCV, 18 }, { UnitType.Terran_Marine, 8 }, { UnitType.Terran_Vulture, 1 } } },
                new() { UnitType = UnitType.Terran_Bunker, UnitThreshold = new() { { UnitType.Terran_SCV, 20 }, { UnitType.Terran_Marine, 8 }, { UnitType.Terran_Vulture, 1 } } },
                new() { UnitType = UnitType.Terran_Supply_Depot, UnitThreshold = new() { { UnitType.Terran_SCV, 20 }, { UnitType.Terran_Marine, 8 }, { UnitType.Terran_Vulture, 1 } } },
                new() { UnitType = UnitType.Terran_Bunker, UnitThreshold = new() { { UnitType.Terran_SCV, 20 }, { UnitType.Terran_Marine, 8 }, { UnitType.Terran_Vulture, 1 } } },
                new() { UnitType = UnitType.Terran_Bunker, UnitThreshold = new() { { UnitType.Terran_SCV, 20 }, { UnitType.Terran_Marine, 8 }, { UnitType.Terran_Vulture, 1 } } },
            };
        }

        public static List<BuildOrderItem> GetAggressiveBuildOrder()
        {
            return new()
            {
                new() { UnitType = UnitType.Terran_Supply_Depot, UnitThreshold = new() { { UnitType.Terran_SCV, 9 } } },
                new() { UnitType = UnitType.Terran_Barracks, UnitThreshold = new() { { UnitType.Terran_SCV, 11 } } },
                new() { UnitType = UnitType.Terran_Barracks, UnitThreshold = new() { { UnitType.Terran_SCV, 12 }, { UnitType.Terran_Marine, 2 } } },
                new() { UnitType = UnitType.Terran_Refinery, UnitThreshold = new() { { UnitType.Terran_SCV, 13 }, { UnitType.Terran_Marine, 4 } } },
                new() { UnitType = UnitType.Terran_Academy, UnitThreshold = new() { { UnitType.Terran_SCV, 14 }, { UnitType.Terran_Marine, 6 } } },
                new() { TechType = TechType.Stim_Packs, UnitThreshold = new() { { UnitType.Terran_SCV, 15 }, { UnitType.Terran_Marine, 10 } } },
                // MISSING: upgrades for infantry weapons and armor, but yolo
                new() { UnitType = UnitType.Terran_Supply_Depot, UnitThreshold = new() { { UnitType.Terran_SCV, 20 }, { UnitType.Terran_Marine, 20 } } }
            };
        }

        public static List<BuildOrderItem> GetEconomicBuildOrder()
        {
            return new()
            {
                new() { UnitType = UnitType.Terran_Supply_Depot, UnitThreshold = new() { { UnitType.Terran_SCV, 9 } } },
                new() { UnitType = UnitType.Terran_Barracks, UnitThreshold = new() { { UnitType.Terran_SCV, 11 } } },
                new() { UnitType = UnitType.Terran_Command_Center, UnitThreshold = new() { { UnitType.Terran_SCV, 14 }, { UnitType.Terran_Marine, 1 } } },
                new() { UnitType = UnitType.Terran_Refinery, UnitThreshold = new() { { UnitType.Terran_SCV, 16 }, { UnitType.Terran_Marine, 2 } } },
                new() { UnitType = UnitType.Terran_Supply_Depot, UnitThreshold = new() { { UnitType.Terran_SCV, 20 }, { UnitType.Terran_Marine, 4 } } },
                new() { UnitType = UnitType.Terran_Factory, UnitThreshold = new() { { UnitType.Terran_SCV, 25 }, { UnitType.Terran_Marine, 6 } } },
                new() { UnitType = UnitType.Terran_Starport, UnitThreshold = new() { { UnitType.Terran_SCV, 30 }, { UnitType.Terran_Marine, 10 } } },
                new() { UnitType = UnitType.Terran_Supply_Depot, UnitThreshold = new() { { UnitType.Terran_SCV, 35 }, { UnitType.Terran_Marine, 12 } } }
            };
        }

        public static List<BuildOrderItem> GetDefensiveBuildOrder()
        {
            return new()
            {
                new() { UnitType = UnitType.Terran_Supply_Depot, UnitThreshold = new() { { UnitType.Terran_SCV, 9 } } },
                new() { UnitType = UnitType.Terran_Barracks, UnitThreshold = new() { { UnitType.Terran_SCV, 11 } } },
                new() { UnitType = UnitType.Terran_Refinery, UnitThreshold = new() { { UnitType.Terran_SCV, 12 } } },
                new() { UnitType = UnitType.Terran_Bunker, UnitThreshold = new() { { UnitType.Terran_SCV, 13 }, { UnitType.Terran_Marine, 2 } } },
                new() { UnitType = UnitType.Terran_Factory, UnitThreshold = new() { { UnitType.Terran_SCV, 15 }, { UnitType.Terran_Marine, 4 } } },
                new() { UnitType = UnitType.Terran_Machine_Shop, UnitThreshold = new() { { UnitType.Terran_SCV, 16 }, { UnitType.Terran_Marine, 6 } } },
                new() { TechType = TechType.Tank_Siege_Mode, UnitThreshold = new() { { UnitType.Terran_SCV, 18 }, { UnitType.Terran_Marine, 8 } } },
                new() { UnitType = UnitType.Terran_Bunker, UnitThreshold = new() { { UnitType.Terran_SCV, 22 }, { UnitType.Terran_Marine, 12 } } },
                new() { UnitType = UnitType.Terran_Bunker, UnitThreshold = new() { { UnitType.Terran_SCV, 25 }, { UnitType.Terran_Marine, 15 } } },
                new() { UnitType = UnitType.Terran_Engineering_Bay, UnitThreshold = new() { { UnitType.Terran_SCV, 30 }, { UnitType.Terran_Marine, 20 } } }
            };
        }

        /// <summary>
        /// It would maintain AT LEAST minimumMineralConfig workers on minerals,
        /// any excess workers would be sent to gas until gasConfig is met,
        /// and any excess workers beyond that would remain on minerals.
        /// </summary>
        /// <param name="totalWorkers"></param>
        /// <param name="gasConfig"></param>
        /// <param name="minimumMineralConfig"></param>
        /// <returns>(mineralWorkers, gasWorkers)</returns>
        public static (int mineral, int gas) DistributeWorkersForGathering(int totalWorkers, int gasConfig, int minimumMineralConfig)
        {
            if (totalWorkers <= minimumMineralConfig)
            {
                return (totalWorkers, 0);
            }

            int gasWorkers = Math.Min(gasConfig, totalWorkers - minimumMineralConfig);
            int mineralWorkers = totalWorkers - gasWorkers;

            return (mineralWorkers, gasWorkers);
        }
    }
}
