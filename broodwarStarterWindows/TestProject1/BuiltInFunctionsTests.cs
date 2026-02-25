using BWAPI.NET;
using Moq;
using Shared.Interfaces;
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

        [Theory]
        [InlineData(UnitType.Terran_Marine, UnitType.Terran_Barracks)]
        [InlineData(UnitType.Terran_SCV, UnitType.Terran_Command_Center)]
        [InlineData(UnitType.Terran_Vulture, UnitType.Terran_Factory)]
        public void CorrectProductionBuildingSourceForUnitType(UnitType unitType, UnitType producingBuildingType)
        {
            unitType.WhatBuilds().First.ShouldBe(producingBuildingType);
        }
    }
}