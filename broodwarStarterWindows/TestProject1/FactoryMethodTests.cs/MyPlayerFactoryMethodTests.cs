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
    public class MyPlayerFactoryMethodTests
    {
        [Fact]
        public void MyPlayerFactoryMethodInitialization_LinksToPlayerData()
        {
            var mockPlayerData = new Mock<IPlayerData>();

            MyPlayer player = MyPlayer.CreateForTesting(mockPlayerData.Object);

            player.PlayerData.ShouldNotBeNull();
            player.PlayerData.ShouldBe(mockPlayerData.Object);
        }
    }
}
