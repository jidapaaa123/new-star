using BWAPI.NET;
using Moq;
using Shared;
using Shared.Interfaces;
using Shared.Models;
using Shouldly;
using Microsoft.Extensions.Logging;
using Shared.Wrappers;

namespace TestProject1.FactoryMethodTests.cs
{
    public class MyGameFactoryMethodTests
    {
        [Fact]
        public void CreateForTesting_Should_Return_MyGame_Instance_With_Mocked_GameData()
        {
            // Arrange
            var mockGameData = new Mock<IGameData>();

            // Act
            MyGame game = MyGame.CreateForTesting(mockGameData.Object);

            // Assert
            game.ShouldNotBeNull();
            game.ShouldBeOfType<MyGame>();
            game.GameData.ShouldBe(mockGameData.Object);
        }
    }
}
