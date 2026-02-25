using BWAPI.NET;
using Moq;
using Shared;
using Shared.Interfaces;
using Shared.Models;
using Shared.Services;
using Shouldly;

namespace TestProject1
{
    public class WorkerDispatcherTests
    {
        private Mock<IMyGame> CreateMockedGameForStrategyInitialization()
        {
            var mockGame = new Mock<IMyGame>();
            var mockPlayer = new Mock<IMyPlayer>();
            var mockBase = new Mock<IMyUnit>();

            mockBase.Setup(b => b.GetTilePosition()).Returns(new TilePosition(0, 0));
            mockPlayer.Setup(p => p.GetBases()).Returns(new List<IMyUnit> { mockBase.Object });
            mockGame.Setup(g => g.Self()).Returns(mockPlayer.Object);

            return mockGame;
        }

        [Fact]
        public void OrderIdleUnitsToGatherMaterials_Should_Send_Available_Workers_To_Gather()
        {
            // ARRANGE
            var dispatcher = new WorkerDispatcher();

            var mockWorker1 = new Mock<IMyUnit>();
            var mockWorker2 = new Mock<IMyUnit>();
            var mockBase = new Mock<IMyUnit>();
            var mockMineral = new Mock<IMyUnit>();

            // Setup worker1
            mockWorker1.Setup(w => w.GetID()).Returns(1);

            // Setup worker2
            mockWorker2.Setup(w => w.GetID()).Returns(2);

            mockBase.Setup(b => b.GetID()).Returns(100);
            mockMineral.Setup(m => m.GetID()).Returns(200);

            var mockPlayer = new Mock<IMyPlayer>();
            mockPlayer.Setup(p => p.GetBases()).Returns(new List<IMyUnit> { mockBase.Object });
            mockBase.Setup(b => b.GetUnitType()).Returns(UnitType.Terran_Command_Center);
            mockPlayer.Setup(p => p.GetWorkerUnits()).Returns(new List<IMyUnit> { mockWorker1.Object, mockWorker2.Object });
            mockPlayer.Setup(p => p.GetUnits()).Returns(new List<IMyUnit> { mockWorker1.Object, mockWorker2.Object, mockBase.Object });

            var mockGame = new Mock<IMyGame>();
            mockGame.Setup(g => g.Self()).Returns(mockPlayer.Object);
            mockGame.Setup(g => g.GetMinerals()).Returns(new List<IMyUnit> { mockMineral.Object });
            mockGame.Setup(g => g.ClosestInstanceOfTo(It.IsAny<List<IMyUnit>>(), It.IsAny<IMyUnit>()))
                .Returns(mockMineral.Object);

            var mockConstructionManager = new Mock<IConstructionManager>();

            // Create GameStrategy using helper method
            var gameForStrategy = CreateMockedGameForStrategyInitialization();
            var strategy = new GameStrategy(gameForStrategy.Object);
            strategy.IdleWorkersSentToGatherMaterials = true;

            // ACT
            dispatcher.OrderIdleUnitsToGatherMaterials(
                mockGame.Object,
                mockConstructionManager.Object,
                strategy);

            // ASSERT
            StaticGameInfo.IsAvailable(mockWorker1.Object).ShouldBeTrue();
            StaticGameInfo.IsAvailable(mockWorker2.Object).ShouldBeTrue();

            mockPlayer.Verify(
                p => p.SendTheseWorkersToGatherAt(
                    It.IsAny<IConstructionManager>(),
                    It.Is<List<IMyUnit>>(workers => workers.Count == 2),
                    mockMineral.Object),
                Times.Once,
                "Should send 2 available workers to gather at minerals");
        }
    }
}