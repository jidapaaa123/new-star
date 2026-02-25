using BWAPI.NET;
using Moq;
using Shared;
using Shared.Interfaces;
using Shouldly;
using System.Collections.ObjectModel;
using System.Security.AccessControl;

namespace TestProject1
{
    public class StaticGameInfoTests
    {
        [Fact]
        public void CorrectDistributionOfGasandMineralWorkers_Excess_AfterGasConfigReached()
        {
            int totalWorkers = 12;
            int gasConfig = 3;
            int minimumMineralConfig = 6;

            (int mineral, int gas) = StaticGameInfo.DistributeWorkersForGathering(totalWorkers, gasConfig, minimumMineralConfig);

            mineral.ShouldBe(9);
            gas.ShouldBe(3);
        }

        [Fact]
        public void CorrectDistributionOfGasandMineralWorkers_LessThanMinimumMineralConfig()
        {
            int totalWorkers = 5;
            int gasConfig = 3;
            int minimumMineralConfig = 6;

            (int mineral, int gas) = StaticGameInfo.DistributeWorkersForGathering(totalWorkers, gasConfig, minimumMineralConfig);

            mineral.ShouldBe(5);
            gas.ShouldBe(0);
        }
    }
}