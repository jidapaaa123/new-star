using Moq;
using MobileApp.Services;
using MobileApp.ViewModels;
using Shouldly;

namespace TestProject1.ViewModelTests
{
    public class DashboardPageViewModelTests
    {
        private readonly Mock<IBotControlService> _mockBotControlService;
        private readonly HttpClient _httpClient;
        private readonly DashboardPageViewModel _viewModel;

        public DashboardPageViewModelTests()
        {
            _mockBotControlService = new Mock<IBotControlService>();
            _httpClient = new HttpClient(new HttpClientHandler { UseProxy = false })
            {
                Timeout = TimeSpan.FromMilliseconds(1)
            };
            _viewModel = new DashboardPageViewModel(_mockBotControlService.Object, _httpClient);
        }

        [Fact]
        public void Constructor_InitializesPropertiesWithDefaultValues()
        {
            // Assert
            _viewModel.IsRunning.ShouldBe(false);
            _viewModel.InGame.ShouldBe(false);
            _viewModel.Supply.ShouldBe(0);
            _viewModel.SelectedStrategy.ShouldBe("Aggressive");
            _viewModel.AvailableStrategies.ShouldContain("Aggressive");
            _viewModel.AvailableStrategies.ShouldContain("Economic");
            _viewModel.AvailableStrategies.ShouldContain("Defensive");
            _viewModel.ShowConfirmation.ShouldBe(false);
            _viewModel.IsLoading.ShouldBe(false);
        }

        [Fact]
        public async Task SetStrategyCommand_ShowsConfirmationMessage_OnSuccess()
        {
            // Arrange
            _mockBotControlService.Setup(s => s.ChangeStrategyAsync(It.IsAny<string>()))
                .ReturnsAsync(true);
            _viewModel.SelectedStrategy = "Economic";

            // Act
            await _viewModel.SetStrategyCommand.ExecuteAsync(null);
            await Task.Delay(100); // Give time for confirmation to show

            // Assert
            _viewModel.ConfirmationMessage.ShouldContain("Economic");
            _viewModel.ShowConfirmation.ShouldBe(true);
        }
    }
}
