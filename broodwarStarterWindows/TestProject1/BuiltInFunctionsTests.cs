using BWAPI.NET;
using Moq;
using Shared.Interfaces;
using Shared.MyLogic;
using Shouldly;
using System.Collections.ObjectModel;
using System.Security.AccessControl;

namespace TestProject1
{
    public class BuiltInFunctionsTests
    {
        [Fact]
        public void BarracksRequiresCommandCenter()
        {
            // --- ARRANGE ---
            var barracksType = UnitType.Terran_Barracks;
            var commandCenterType = UnitType.Terran_Command_Center;
            // --- ACT ---
            ReadOnlyDictionary<UnitType, int> requiredBuildings = barracksType.RequiredUnits();
            // --- ASSERT ---
            requiredBuildings.Count.ShouldBe(1);
            requiredBuildings.ContainsKey(commandCenterType).ShouldBeTrue();
            requiredBuildings[commandCenterType].ShouldBe(1);
        }

        [Fact]
        public void StarportRequiresFactory()
        {
            // --- ARRANGE ---
            var starportType = UnitType.Terran_Starport;
            var factoryType = UnitType.Terran_Factory;
            // --- ACT ---
            ReadOnlyDictionary<UnitType, int> requiredBuildings = starportType.RequiredUnits();
            // --- ASSERT ---
            requiredBuildings.Count.ShouldBe(1);
            requiredBuildings.ContainsKey(factoryType).ShouldBeTrue();
            requiredBuildings[factoryType].ShouldBe(1);
        }
    }
}