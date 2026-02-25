using Moq;
using MobileApp.Services;
using MobileApp.ViewModels;
using Shouldly;

namespace TestProject1.ViewModelTests
{
    public class EventsPageViewModelTests
    {
        private readonly Mock<IBotControlService> _mockBotControlService;
        private readonly EventsPageViewModel _viewModel;

        public EventsPageViewModelTests()
        {
            _mockBotControlService = new Mock<IBotControlService>();
            _viewModel = new EventsPageViewModel(_mockBotControlService.Object);
        }

        [Fact]
        public void Constructor_InitializesPropertiesWithDefaultValues()
        {
            // Assert
            _viewModel.IsLoading.ShouldBe(false);
            _viewModel.Events.ShouldBeEmpty();
            _viewModel.HasNoEvents.ShouldBe(true);
        }

        [Fact]
        public async Task RefreshEventsCommand_DisplaysEventsCorrectly_WhenEventsExist()
        {
            // Arrange
            var testEvents = new List<(string EventType, string Description, DateTime Timestamp)>
            {
                ("expansion", "Expansion started at position (50, 50)", DateTime.UtcNow.AddSeconds(-10)),
                ("scout", "Enemy found at location (100, 100)", DateTime.UtcNow)
            };

            _mockBotControlService.Setup(s => s.GetLatestMatchIdAsync())
                .ReturnsAsync(1);
            _mockBotControlService.Setup(s => s.GetMatchEventsAsync(It.IsAny<int>()))
                .ReturnsAsync(testEvents);

            // Act
            await _viewModel.RefreshEventsCommand.ExecuteAsync(null);

            // Assert
            _viewModel.Events.Count.ShouldBe(2);
            _viewModel.HasNoEvents.ShouldBe(false);
            _viewModel.Events[0].EventType.ShouldBe("scout"); // Most recent first due to OrderByDescending
        }
    }
}
