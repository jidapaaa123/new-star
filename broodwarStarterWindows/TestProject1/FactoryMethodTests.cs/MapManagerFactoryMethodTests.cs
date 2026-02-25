using BWAPI.NET;
using Moq;
using Shared.Interfaces;
using Shared.Models;
using Shared.Wrappers;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Text;

namespace TestProject1.FactoryMethodTests.cs
{
    public class MapManagerFactoryMethodTests
    {
        [Fact]
        public void MapManagerFactoryMethodInitialization_LinksToGameObject_MapNotInitialized()
        {
            var mockGameData = new Mock<IGameData>();
            MapManager mapManager = MapManager.CreateForTesting(mockGameData.Object);

            mapManager.ShouldNotBeNull();
            mapManager.ShouldBeOfType<MapManager>();
            mapManager.GameData.ShouldBe(mockGameData.Object);

            mapManager.IsInitialized.ShouldBeFalse();
        }
    }
}
