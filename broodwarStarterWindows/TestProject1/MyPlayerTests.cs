using BWAPI.NET;
using Moq;
using Shared;
using Shared.Interfaces;
using Shared.Models;
using Shouldly;
using Microsoft.Extensions.Logging;
using Shared.Wrappers;

namespace TestProject1
{
    public class MyPlayerTests
    {
        [Fact]
        public void TotalSCVIncludingInQueue_CalculatesCorrectly()
        {
            // ARRANGE
            var mockUnit1 = new Mock<IUnitData>();
            var mockUnit2 = new Mock<IUnitData>();
            var mockProductionBuilding = new Mock<IUnitData>();
            int unitsInQueue = 3;
            
            mockUnit1.Setup(u => u.GetUnitType()).Returns(UnitType.Terran_SCV);
            mockUnit2.Setup(u => u.GetUnitType()).Returns(UnitType.Terran_SCV);
            
            mockProductionBuilding.Setup(u => u.GetUnitType()).Returns(UnitType.Terran_Command_Center);
            mockProductionBuilding.Setup(u => u.GetTrainingQueue()).Returns(Enumerable.Repeat(UnitType.Terran_SCV, unitsInQueue).ToList());
            
            var mockPlayerData = new Mock<IPlayerData>();
            mockPlayerData.Setup(p => p.GetUnits()).Returns(new List<IUnitData> 
            { 
                mockUnit1.Object, 
                mockUnit2.Object, 
                mockProductionBuilding.Object 
            });
            
            var playerAdapter = new MyPlayer(mockPlayerData.Object);
            
            // ACT & ASSERT
            int totalSCVs = playerAdapter.TotalUnitsIncludingInQueue(UnitType.Terran_SCV);
            totalSCVs.ShouldBe(5); // 2 existing SCVs + 3 in training queue
        }
    }
}


