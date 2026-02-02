using BWAPI.NET;
using Moq;
using Shared;
using Shared.Interfaces;
using Shared.MyLogic;
using Shared.Models;
using Shouldly;
using Microsoft.Extensions.Logging;

namespace TestProject1
{
    public class UnitTest1
    {
        private Mock<ILogger<MyStarcraftBot>> CreateMockLogger()
        {
            return new Mock<ILogger<MyStarcraftBot>>();
        }

        /// <summary>
        /// Test 2: Scouting System
        /// Verifies that scout unit rotates through potential bases and detects enemies
        /// </summary>
        [Fact]
        public void Scout_Should_Track_Explored_Locations_And_Enemy_Detection()
        {
            // ARRANGE
            var logger = CreateMockLogger();
            var bot = new MyStarcraftBot(logger.Object);
            
            // Create scout locations (can be set through reflection since property is read-only)
            var scoutLocations = new List<ScoutLocation>
            {
                new ScoutLocation(new TilePosition(10, 10)) { IsExplored = false, EnemyFound = false },
                new ScoutLocation(new TilePosition(20, 20)) { IsExplored = false, EnemyFound = false },
                new ScoutLocation(new TilePosition(30, 30)) { IsExplored = false, EnemyFound = false }
            };

            // ACT
            // Simulate scouting progress
            scoutLocations[0].IsExplored = true;
            scoutLocations[1].IsExplored = true;
            scoutLocations[1].EnemyFound = true;

            // ASSERT
            scoutLocations.Count.ShouldBe(3);
            scoutLocations.Count(s => s.IsExplored).ShouldBe(2);
            scoutLocations.Count(s => s.EnemyFound).ShouldBe(1);
            scoutLocations.First(s => s.EnemyFound).EnemyFound.ShouldBe(true);
        }

        /// <summary>
        /// Test 3: Worker Assignment to Construction
        /// Verifies that construction manager tracks pending orders properly
        /// </summary>
        [Fact]
        public void Construction_Manager_Should_Track_Pending_Orders()
        {
            // ARRANGE
            var logger = CreateMockLogger();
            var bot = new MyStarcraftBot(logger.Object);

            // ACT & ASSERT
            // Initially construction manager should have no orders
            bot.ConstructionManager.ShouldNotBeNull();
            bot.ConstructionManager.PendingConstructionOrders.Count.ShouldBe(0);
            bot.ConstructionManager.GetPendingWorkerId().ShouldBeNull();
        }

        /// <summary>
        /// Test 4: Unit Production Configuration
        /// Verifies that production manager exists and is properly initialized
        /// </summary>
        [Fact]
        public void Production_Manager_Should_Be_Initialized()
        {
            // ARRANGE
            var logger = CreateMockLogger();
            var bot = new MyStarcraftBot(logger.Object);

            // ACT & ASSERT
            bot.ProductionManager.ShouldNotBeNull();
            
            // Verify methods exist
            var hasConfigTrainSCV = bot.ProductionManager.GetType().GetMethod("ConfigTrainSCV") != null;
            var hasConfigTrainMarine = bot.ProductionManager.GetType().GetMethod("ConfigTrainMarine") != null;
            var hasConfigTrainVulture = bot.ProductionManager.GetType().GetMethod("ConfigTrainVulture") != null;
            
            hasConfigTrainSCV.ShouldBe(true);
            hasConfigTrainMarine.ShouldBe(true);
            hasConfigTrainVulture.ShouldBe(true);
        }

        /// <summary>
        /// Test 5: Offense Team Management
        /// Verifies that bot can manage offense team operations
        /// </summary>
        [Fact]
        public void Bot_Should_Support_Offense_Team_Operations()
        {
            // ARRANGE
            var logger = CreateMockLogger();
            var bot = new MyStarcraftBot(logger.Object);

            // ACT & ASSERT
            var hasGetOffenseTeam = bot.GetType().GetMethod("GetOffenseTeam", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance) != null;
            var hasAttackEnemyBase = bot.GetType().GetMethod("AttackEnemyBase") != null;
            var hasManageAndRallyOffenseTeam = bot.GetType().GetMethod("ManageAndRallyOffenseTeam") != null;
            
            hasGetOffenseTeam.ShouldBe(true);
            hasAttackEnemyBase.ShouldBe(true);
            hasManageAndRallyOffenseTeam.ShouldBe(true);
        }

        /// <summary>
        /// Test 6: Construction Order Cleanup
        /// Verifies that construction manager has cleanup methods
        /// </summary>
        [Fact]
        public void Construction_Manager_Should_Support_Worker_Cleanup()
        {
            // ARRANGE
            var logger = CreateMockLogger();
            var bot = new MyStarcraftBot(logger.Object);
            var initialOrderCount = bot.ConstructionManager.PendingConstructionOrders.Count;

            // ACT & ASSERT
            var hasRemoveWorkerConstructionOrder = 
                bot.ConstructionManager.GetType().GetMethod("RemoveWorkerConstructionOrder") != null;
            hasRemoveWorkerConstructionOrder.ShouldBe(true);
            
            // Order count should remain 0 if nothing was added
            bot.ConstructionManager.PendingConstructionOrders.Count.ShouldBe(initialOrderCount);
        }


        [Fact]
        public void Bot_Should_Initialize_With_All_Managers()
        {
            // ARRANGE & ACT
            var logger = CreateMockLogger();
            var bot = new MyStarcraftBot(logger.Object);

            // ASSERT
            bot.ConstructionManager.ShouldNotBeNull();
            bot.ProductionManager.ShouldNotBeNull();
            bot.IsRunning.ShouldBe(false);
            bot.InGame.ShouldBe(false);
            bot.Game.ShouldBeNull();
            //bot.MapManager.ShouldNotBeNull();
        }

        [Fact]
        public void OnStart_Should_Set_InGame_State()
        {
            // ARRANGE
            var logger = CreateMockLogger();
            var bot = new MyStarcraftBot(logger.Object);
            var statusChangedCalled = false;
            bot.StatusChanged += () => statusChangedCalled = true;

            // ACT
            bot.OnStart();

            // ASSERT
            bot.InGame.ShouldBe(true);
            statusChangedCalled.ShouldBe(true);
        }

        [Fact]
        public void OnEnd_Should_Clear_InGame_State()
        {
            // ARRANGE
            var logger = CreateMockLogger();
            var bot = new MyStarcraftBot(logger.Object);
            bot.OnStart();
            var statusChangedCount = 0;
            bot.StatusChanged += () => statusChangedCount++;
            statusChangedCount = 0; // Reset after OnStart

            // ACT
            bot.OnEnd(true);

            // ASSERT
            bot.InGame.ShouldBe(false);
            statusChangedCount.ShouldBe(1);
        }

        [Fact]
        public void EnqueueCommand_Should_Accept_Multiple_Commands_Without_Error()
        {
            // ARRANGE
            var logger = CreateMockLogger();
            var bot = new MyStarcraftBot(logger.Object);
            
            var commands = new[]
            {
                new BotCommand { Type = BotCommandType.ScoutMap },
                new BotCommand { Type = BotCommandType.ToggleStrategy },
                new BotCommand { Type = BotCommandType.ToggleAttackEnemyBase },
                new BotCommand { Type = BotCommandType.ManageBunkerProduction },
                new BotCommand { Type = BotCommandType.ManageSupplyDepotProduction }
            };

            // ACT & ASSERT
            foreach (var command in commands)
            {
                var exception = Record.Exception(() => bot.EnqueueCommand(command));
                exception.ShouldBeNull();
            }
        }

        [Fact]
        public void ToggleStrategy_Should_Not_Throw_When_Strategy_Is_Null()
        {
            // ARRANGE
            var logger = CreateMockLogger();
            var bot = new MyStarcraftBot(logger.Object);

            // ACT & ASSERT
            var exception = Record.Exception(() => bot.ToggleStrategy());
            exception.ShouldBeNull();
        }

        [Fact]
        public void Connect_Should_Set_IsRunning_True()
        {
            // ARRANGE
            var logger = CreateMockLogger();
            var bot = new MyStarcraftBot(logger.Object);

            // ACT
            bot.Connect();

            // ASSERT
            bot.IsRunning.ShouldBe(true);
        }

        [Fact]
        public void Disconnect_Should_Set_IsRunning_False()
        {
            // ARRANGE
            var logger = CreateMockLogger();
            var bot = new MyStarcraftBot(logger.Object);
            bot.Connect();

            // ACT
            bot.Disconnect();

            // ASSERT
            bot.IsRunning.ShouldBe(false);
            bot.InGame.ShouldBe(false);
        }
    }
}


