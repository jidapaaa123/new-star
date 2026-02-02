using BWAPI.NET;
using Shared.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Models
{
    public class BuildOrderItem
    {
        public UnitType UnitType { get; set; }
        public int SCVThreshold { get; set; } = 0;
        public int MarineThreshold { get; set; } = 0;
        public int VultureThreshold { get; set; } = 0;

    }
    public class GameStrategy
    {
        public IMyGame GameAdapter { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IdleWorkersSentToGatherMaterials { get; set; }
        public int GasGatherConfig { get; set; }
        public int SCVConfig { get; set; }
        public int MarineConfig { get; set; }
        public int VultureConfig { get; private set; }


        public TilePosition InitialPosition { get; set; }
        public int MaxRange { get; set; }

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

        public GameStrategy(IMyGame game)
        {
            GameAdapter = game;
            Description = "Default Strategy: Research Cloaking Fields, Produce Wraiths and Science Vessels";
            Name = "Default";
            IdleWorkersSentToGatherMaterials = true;
            BuildOrderItems = new()
            {
                new() { UnitType = UnitType.Terran_Supply_Depot, SCVThreshold = 9, MarineThreshold = 0 },
                new() { UnitType = UnitType.Terran_Barracks,     SCVThreshold = 11, MarineThreshold = 0 },
                new() { UnitType = UnitType.Terran_Refinery,     SCVThreshold = 13, MarineThreshold = 2 },

                new() { UnitType = UnitType.Terran_Supply_Depot, SCVThreshold = 13, MarineThreshold = 2 },

                // Mid-game: Balancing tech with military unit counts
                new() { UnitType = UnitType.Terran_Engineering_Bay, SCVThreshold = 15, MarineThreshold = 5 },
                new() { UnitType = UnitType.Terran_Factory,         SCVThreshold = 16, MarineThreshold = 8 },
    
                new() { UnitType = UnitType.Terran_Supply_Depot, SCVThreshold = 16, MarineThreshold = 8 },
    
                // Late-tech: Pushing to the 15/15 requirement
                new() { UnitType = UnitType.Terran_Starport,        SCVThreshold = 17, MarineThreshold = 8 },
                new() { UnitType = UnitType.Terran_Control_Tower,   SCVThreshold = 18, MarineThreshold = 8, VultureThreshold = 1 },

                new() { UnitType = UnitType.Terran_Supply_Depot, SCVThreshold = 18, MarineThreshold = 8, VultureThreshold = 1 },

                new() { UnitType = UnitType.Terran_Science_Facility, SCVThreshold = 20, MarineThreshold = 8, VultureThreshold = 1 },

                new() { UnitType = UnitType.Terran_Supply_Depot, SCVThreshold = 20, MarineThreshold = 8, VultureThreshold = 1 },

            };

            var bases = game.Self().GetBases();
            InitialPosition = bases[0].GetPosition().ToTilePosition();
            MaxRange = 64;
            GasGatherConfig = 3;

            SCVConfig = 7;
            MarineConfig = 0;
            VultureConfig = 0;
            IsPaused = false;
        }

        public void CompletedBuildOrderStep()
        {
            CurrentBuildOrderIndex++;
            WorkerAssignedToCurrentStep = false;
        }

        public void SetWorkerAssignedToCurrentStep()
        {
            WorkerAssignedToCurrentStep = true;
            var currentBuildOrderStep = BuildOrderItems[CurrentBuildOrderIndex];

            SCVConfig = currentBuildOrderStep.SCVThreshold;
            MarineConfig = currentBuildOrderStep.MarineThreshold;
            VultureConfig = currentBuildOrderStep.VultureThreshold;
        }
    }
}
