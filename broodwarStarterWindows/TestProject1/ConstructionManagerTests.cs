using BWAPI.NET;
using Moq;
using Shared;
using Shared.Interfaces;
using Shared.Models;
using Shared.Wrappers;
using Shouldly;

namespace TestProject1
{
    public class ConstructionManagerTests
    {
        [Fact]
        public void RemoveWorkerConstructionOrder_Should_Return_The_Removed_Order()
        {
            // ARRANGE
            var constructionManager = new ConstructionManager();
            var mockWorkerData = new Mock<IUnitData>();
            mockWorkerData.Setup(w => w.GetUnitType()).Returns(UnitType.Terran_SCV);
            var worker = MyUnit.CreateForTesting(mockWorkerData.Object);

            var expectedOrder = new ConstructionOrder
            {
                BuildingType = UnitType.Terran_Supply_Depot,
                Worker = worker,
                Costs = new Materials { Minerals = 100, Gas = 0 },
                TilePosition = new TilePosition(20, 20),
                IsFromBuildOrder = false
            };

            constructionManager.RegisterOrder(expectedOrder);

            // ACT
            var removedOrder = constructionManager.RemovePendingConstructionOrder();

            // ASSERT
            constructionManager.PendingConstructionOrder.ShouldBeNull();

            removedOrder.ShouldBeSameAs(expectedOrder);
            removedOrder.BuildingType.ShouldBe(UnitType.Terran_Supply_Depot);
            removedOrder.Costs.Minerals.ShouldBe(100);
            removedOrder.TilePosition.X.ShouldBe(20);
            removedOrder.TilePosition.Y.ShouldBe(20);
        }

        [Fact]
        public void RemoveWorkerConstructionOrder_ShouldFlagUnit_IsInConstructionOrder_False()
        {
            // ARRANGE
            var constructionManager = new ConstructionManager();
            var mockWorkerData = new Mock<IUnitData>();
            mockWorkerData.Setup(w => w.GetUnitType()).Returns(UnitType.Terran_SCV);
            var worker = MyUnit.CreateForTesting(mockWorkerData.Object);

            var order = new ConstructionOrder { BuildingType = UnitType.Terran_Barracks, Worker = worker };
            constructionManager.RegisterOrder(order);
            worker.IsInConstructionManager().ShouldBeTrue();

            // ACT
            constructionManager.RemovePendingConstructionOrder();
            // ASSERT
            constructionManager.PendingConstructionOrder.ShouldBeNull();
            worker.IsInConstructionManager().ShouldBeFalse();
        }

        [Fact]
        public void RegisterOrder_ShouldFlagUnit_IsInConstructionOrder_True()
        {
            // ARRANGE
            var constructionManager = new ConstructionManager();
            var mockWorkerData = new Mock<IUnitData>();
            mockWorkerData.Setup(w => w.GetUnitType()).Returns(UnitType.Terran_SCV);
            var worker = MyUnit.CreateForTesting(mockWorkerData.Object);  

            // ACT
            constructionManager.RegisterOrder(UnitType.Terran_Barracks, worker, new TilePosition(10, 10), false);

            // ASSERT
            worker.IsInConstructionManager().ShouldBeTrue();
            constructionManager.PendingConstructionOrder.ShouldNotBeNull();
        }
    }
}
