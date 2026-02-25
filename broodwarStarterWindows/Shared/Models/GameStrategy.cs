using BWAPI.NET;
using Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Models
{
    public class GameStrategy
    {
        public IMyGame GameAdapter { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IdleWorkersSentToGatherMaterials { get; set; }
        public int GasGatherConfig { get; set; }
        public int MinimumMineralGatherConfig { get; set; }


        public TilePosition InitialPosition { get; set; }
        public int MaxRange { get; set; }
        public Strategy CurrentStrategy { get; private set; }

        /// <summary>
        /// Build order. Order matters. Can specify supply count to trigger each step. (or leave null)
        /// </summary>
        /// <remarks>Each tuple in the list represents a build order step: the first item is the unit type
        /// to produce, and the second item is the supply count at which to trigger production. If the supply count is
        /// null, it will repeatedly try to build until it successfully orders construction.</remarks>
        public List<BuildOrderItem> BuildOrderItems { get; set; } = new();
        public int CurrentBuildOrderIndex { get; set; } = 0;
        public bool WorkerAssignedToCurrentStep { get; set; } = false;
        public bool IsPaused { get; set; }
        
        /// <summary>
        /// Tracks units/buildings completed from the build order to avoid duplicates when switching strategies
        /// </summary>
        public Dictionary<UnitType, int> CompletedBuildOrderUnits { get; set; } = new();

        public GameStrategy(IMyGame game)
        {
            GameAdapter = game;
            var bases = game.Self().GetBases();
            InitialPosition = bases[0].GetPosition().ToTilePosition();
            MaxRange = 64;
            IsPaused = false;

            // Configure default strategy
            ConfigureStrategySettings(Strategy.Default);
            BuildOrderItems = StaticGameInfo.GetDefaultBuildOrder();
        }

        private void ConfigureStrategySettings(Strategy strategyType)
        {
            CurrentStrategy = strategyType;
            
            switch (strategyType)
            {
                case Strategy.Aggressive:
                    Name = "Aggressive";
                    Description = "Aggressive Strategy: Rush with early military";
                    IdleWorkersSentToGatherMaterials = true;
                    GasGatherConfig = 2;      // Less gas, focus on military
                    MinimumMineralGatherConfig = 3;
                    break;

                case Strategy.Economic:
                    Name = "Economic";
                    Description = "Economic Strategy: Focus on economy and expansion";
                    IdleWorkersSentToGatherMaterials = true;
                    GasGatherConfig = 4;      // More gas for tech and expansion
                    MinimumMineralGatherConfig = 8;
                    break;

                case Strategy.Defensive:
                    Name = "Defensive";
                    Description = "Defensive Strategy: Build defenses and tech up";
                    IdleWorkersSentToGatherMaterials = true;
                    GasGatherConfig = 3;      // Balanced gas for tech
                    MinimumMineralGatherConfig = 5;
                    break;

                case Strategy.Default:
                default:
                    Name = "Default";
                    Description = "Default Strategy: Research Cloaking Fields, Produce Wraiths and Science Vessels";
                    IdleWorkersSentToGatherMaterials = true;
                    GasGatherConfig = 3;
                    MinimumMineralGatherConfig = 5;
                    break;
            }
        }

        public void ChangeStrategy(Strategy name)
        { 
            var newOrder = name switch
            {
                Strategy.Aggressive => StaticGameInfo.GetAggressiveBuildOrder(),
                Strategy.Defensive => StaticGameInfo.GetDefensiveBuildOrder(),
                Strategy.Economic => StaticGameInfo.GetEconomicBuildOrder(),
                Strategy.Default => StaticGameInfo.GetDefaultBuildOrder(),
                _ => throw new ArgumentException("Invalid strategy name", nameof(name))
            };

            BuildOrderItems = newOrder;
            ConfigureStrategySettings(name);
            
            // Find the first item in new build order we haven't completed yet
            CurrentBuildOrderIndex = FindNextUncompletedStep();
            WorkerAssignedToCurrentStep = false;
        }

        private int FindNextUncompletedStep()
        {
            // Create a working copy of completed units
            var remainingUnits = new Dictionary<UnitType, int>(CompletedBuildOrderUnits);
            
            for (int i = 0; i < BuildOrderItems.Count; i++)
            {
                var item = BuildOrderItems[i];
                
                // Check if we have a completed unit of this type to "consume"
                if (remainingUnits.TryGetValue(item.UnitType, out int count) && count > 0)
                {
                    remainingUnits[item.UnitType]--;
                }
                else
                {
                    // This is the first item we haven't built yet
                    return i;
                }
            }
            
            // All items completed
            return BuildOrderItems.Count;
        }

        public void CompletedBuildOrderStep()
        {
            if (CurrentBuildOrderIndex < BuildOrderItems.Count)
            {
                var completedUnit = BuildOrderItems[CurrentBuildOrderIndex].UnitType;
                
                // Track that we completed this unit
                if (CompletedBuildOrderUnits.ContainsKey(completedUnit))
                {
                    CompletedBuildOrderUnits[completedUnit]++;
                }
                else
                {
                    CompletedBuildOrderUnits[completedUnit] = 1;
                }
            }
            
            CurrentBuildOrderIndex++;
            WorkerAssignedToCurrentStep = false;
        }

        public void SetWorkerAssignedToCurrentStep()
        {
            WorkerAssignedToCurrentStep = true;
            var currentBuildOrderStep = BuildOrderItems[CurrentBuildOrderIndex];
        }

        public Dictionary<UnitType, int>? GetCurrentUnitThresholds()
        {
            try
            {
                return BuildOrderItems[CurrentBuildOrderIndex].UnitThreshold;

            }
            catch (ArgumentOutOfRangeException)
            {
                return null;
            }
        }

        /// <summary>
        /// Gets the next uncompleted build order item, skipping over any items that have already been completed.
        /// </summary>
        /// <returns>The next BuildOrderItem to complete, or null if the build order is complete.</returns>
        public BuildOrderItem? GetCurrentBuildOrderItem()
        {
            // Find the next item we haven't completed yet
            var nextUncompletedIndex = FindNextUncompletedStep();
            
            if (nextUncompletedIndex >= BuildOrderItems.Count)
                return null;

            // Update the index if it's behind
            CurrentBuildOrderIndex = nextUncompletedIndex;
            return BuildOrderItems[CurrentBuildOrderIndex];
        }

        /// <summary>
        /// Gets the item at the current build order index without checking for completed steps.
        /// Used for initialization and special cases.
        /// </summary>
        private BuildOrderItem? GetCurrentBuildOrderItemDirect()
        {
            if (CurrentBuildOrderIndex >= BuildOrderItems.Count)
                return null;

            return BuildOrderItems[CurrentBuildOrderIndex];
        }

        /// <summary>
        /// Checks if the build order is complete.
        /// </summary>
        /// <returns>True if all build order items have been completed.</returns>
        public bool IsBuildOrderComplete()
        {
            return CurrentBuildOrderIndex >= BuildOrderItems.Count;
        }

        /// <summary>
        /// Inserts a Supply Depot build order if current supply is within 2 of max supply.
        /// Avoids duplicate Supply Depot orders by checking:
        /// 1. The next queued item
        /// 2. Currently incomplete Supply Depots under construction
        /// </summary>
        public void InsertSupplyDepotIfLow(int currentSupply, int maxSupply)
        {
            // Only insert if we're within 2 of max supply but not already at max
            if (currentSupply < maxSupply - 2 || currentSupply >= maxSupply)
                return;

            // Check if there's already a Supply Depot under construction
            var player = GameAdapter.Self();
            if (player.HasIncompleteUnitOfType(UnitType.Terran_Supply_Depot))
                return;

            // Find the next uncompleted step
            var nextUncompletedIndex = FindNextUncompletedStep();
            
            // Let it be if the next item is already a Supply Depot
            if (nextUncompletedIndex < BuildOrderItems.Count && 
                BuildOrderItems[nextUncompletedIndex].UnitType == UnitType.Terran_Supply_Depot)
            {
                return;
            }

            // if not, insert right before the next uncompleted step
            var supplyDepot = new BuildOrderItem { UnitType = UnitType.Terran_Supply_Depot };
            BuildOrderItems.Insert(nextUncompletedIndex, supplyDepot);
        }

        /// <summary>
        /// Inserts a build order item at the current index (for expansion or urgent builds).
        /// </summary>
        /// <param name="item">The BuildOrderItem to insert.</param>
        public void InsertBuildOrderItemAtCurrentIndex(BuildOrderItem item)
        {
            BuildOrderItems.Insert(CurrentBuildOrderIndex, item);
        }
    }

    public enum Strategy { Default, Aggressive, Defensive, Economic }
}
