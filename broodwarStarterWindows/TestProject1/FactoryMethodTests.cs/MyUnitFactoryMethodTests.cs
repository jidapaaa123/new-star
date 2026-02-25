using BWAPI.NET;
using Moq;
using Shared.Interfaces;
using Shared.Wrappers;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Text;

namespace TestProject1.FactoryMethodTests.cs
{
    public class MyUnitFactoryMethodTests
    {
        [Fact]
        public void MyUnitFactoryMethodInitialization_LinksToUnitData()
        {
            var mockUnitData = new Mock<IUnitData>();

            MyUnit unit = MyUnit.CreateForTesting(mockUnitData.Object);

            unit.ShouldNotBeNull();
            unit.ShouldBeOfType<MyUnit>();
            unit.UnitData.ShouldBe(mockUnitData.Object);
        }
    }
}
