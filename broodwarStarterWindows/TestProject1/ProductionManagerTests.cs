using BWAPI.NET;
using Moq;
using Shared;
using Shared.Interfaces;
using Shared.Models;
using Shared.Wrappers;
using Shouldly;
using Microsoft.Extensions.Logging;

namespace TestProject1
{
    public class ProductionManagerTests
    {
        [Fact]
        public void ConfigTrainSCV_CallsTrainOnce_Whenever_Under_Threshold()
        {
            // ARRANGE
            var type = UnitType.Terran_SCV;

            // Mock base must implement BOTH IUnitData (data layer) and IMyUnit (logic layer)
            // because MyPlayer.GetUnits() filters IUnitData with .OfType<IMyUnit>()
            // so if mockBase is only IUnitData, it won't count
            var mockBase = new Mock<IMyUnit>();
            mockBase.Setup(b => b.GetUnitType()).Returns(UnitType.Terran_Command_Center);
            var mockPlayerData = new Mock<IPlayerData>();
            // Return mocked IMyUnit as IUnitData (works since IMyUnit is-a IUnitData via dual implementation)
            mockPlayerData.Setup(p => p.GetUnits()).Returns(new List<IUnitData> { mockBase.As<IUnitData>().Object });

            var mockGameData = new Mock<IGameData>();
            mockGameData.Setup(g => g.Self()).Returns(mockPlayerData.Object);

            var myGame = new MyGame(mockGameData.Object);
            var mockConstructionManager = new Mock<IConstructionManager>();
            var mockProductionManager = new Mock<IProductionManager>();

            var gameStrategy = new GameStrategy(myGame);

            // ACT
            mockProductionManager.Object.ConfigTrainType(type, myGame, mockConstructionManager.Object, gameStrategy);

            // ASSERT
            gameStrategy.CurrentBuildOrderIndex.ShouldBe(0);

            var thresholds = gameStrategy.GetCurrentUnitThresholds();
            thresholds.ShouldContainKey(type);
            thresholds[type].ShouldBe(9); // default threshold for SCV

            mockProductionManager.Verify(m => m.ConfigTrainType(type, myGame, mockConstructionManager.Object, gameStrategy), Times.Once);
        }
    }
}





