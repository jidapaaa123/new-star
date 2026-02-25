using BWAPI.NET;
using Microsoft.Extensions.Logging;
using Moq;
using Shared;
using Shared.DataAdapters;
using Shared.Interfaces;
using Shared.Models;
using Shared.Wrappers;
using Shouldly;

namespace TestProject1
{
    public class ScoutingManagerTests
    {
        private ScoutingManager scoutingManagerFactoryOneAvailableWorkerAnd3ScoutingTargets()
        {
            // WORKER UNITS - Create mock worker unit
            var mockWorkerData = new Mock<IUnitData>();
            var mockConstructionManager = new Mock<IConstructionManager>();
            mockWorkerData.Setup(w => w.GetUnitType()).Returns(UnitType.Terran_SCV);
            mockWorkerData.Setup(w => w.GetID()).Returns(1);
            mockWorkerData.Setup(w => w.GetDistance(It.IsAny<Position>())).Returns(0);

            // Create a MyUnit wrapper that implements both IMyUnit and IUnitData
            var workerUnit = MyUnit.CreateForTesting(mockWorkerData.Object);

            // SCOUT LOCATIONS - Mock GetScoutingTargets to return a list of locations
            var scoutLocations = new List<TilePosition>
            {
                new TilePosition(10, 10),
                new TilePosition(20, 20),
                new TilePosition(30, 30)
            };
            var mockMapManager = new Mock<IMapManager>();
            mockMapManager.Setup(m => m.GetScoutingTargets()).Returns(scoutLocations);

            // PLAYER DATA - Mock player's worker units and other relevant data
            var mockPlayerData = new Mock<IPlayerData>();
            mockPlayerData.Setup(p => p.GetUnits()).Returns(new List<IUnitData> { workerUnit });

            var mockGameData = new Mock<IGameData>();
            mockGameData.Setup(g => g.Self()).Returns(mockPlayerData.Object);
            
            // Mock the enemy player
            var mockEnemyData = new Mock<IPlayerData>();
            mockEnemyData.Setup(e => e.GetUnits()).Returns(new List<IUnitData>());
            mockGameData.Setup(g => g.Enemy()).Returns(mockEnemyData.Object);

            var game = MyGame.CreateForTesting(mockGameData.Object);
            
            return new ScoutingManager(game, mockMapManager.Object);
        }

        [Fact]
        public void ScoutingManagerInitialization_LinksToMyGameObject()
        {
            var mockGameData = new Mock<IGameData>();
            var mockMapManager = new Mock<IMapManager>();
            var game = MyGame.CreateForTesting(mockGameData.Object);

            var scoutingManager = new ScoutingManager(game, mockMapManager.Object);

            scoutingManager.Game.ShouldNotBeNull();
            scoutingManager.Game.ShouldBe(game);
        }

        [Fact]
        public void TestSetupMethod_HasCorrectStructure()
        {
            var sm = scoutingManagerFactoryOneAvailableWorkerAnd3ScoutingTargets();
            int expectedUnits = 1;
            int expectedScoutingTargets = 3;

            sm.PotentialBases?.Count.ShouldBe(expectedScoutingTargets);
            sm.Game.ShouldNotBeNull();
            var player = sm.Game.Self();
            player.ShouldNotBeNull();
            var units = player.GetUnits();
            units.Count().ShouldBe(expectedUnits);
        }

        [Fact]
        public void UpdateScouting_EnableDisableCycle()
        {
            var sm = scoutingManagerFactoryOneAvailableWorkerAnd3ScoutingTargets();
            bool enableScouting = true;

            // Begin Scout Cycle
            sm.UpdateScouting(enableScouting);  
            sm.ScoutUnit.ShouldNotBeNull();
            sm.IsScoutingEnabled.ShouldBeTrue();

            // Simulate exploration
            foreach (var location in sm.PotentialBases ?? new List<ScoutLocation>())
            {
                sm.UpdateScouting(enableScouting);  
            }

            // End Scout Cycle
            enableScouting = false;
            sm.UpdateScouting(enableScouting);
            sm.ScoutUnit.ShouldBeNull();
        }

    }
}
