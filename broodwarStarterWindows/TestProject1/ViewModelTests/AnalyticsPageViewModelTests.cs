using Moq;
using MobileApp.Services;
using MobileApp.ViewModels;
using Shouldly;

namespace TestProject1.ViewModelTests
{
    public class AnalyticsPageViewModelTests
    {
        private readonly Mock<ICacheService> _mockCacheService;
        private readonly AnalyticsPageViewModel _viewModel;

        public AnalyticsPageViewModelTests()
        {
            _mockCacheService = new Mock<ICacheService>();
            // Create a minimal HttpClient configured for testing
            var httpClient = new HttpClient(new HttpClientHandler { UseProxy = false }) 
            { 
                Timeout = TimeSpan.FromMilliseconds(1) 
            };
            _viewModel = new AnalyticsPageViewModel(httpClient, _mockCacheService.Object);
        }

        [Fact]
        public void Constructor_InitializesProperties_WithDefaultValues()
        {
            // Assert
            _viewModel.TotalMatches.ShouldBe(0);
            _viewModel.WinRatePercentage.ShouldBe(0);
            _viewModel.AverageDuration.ShouldBe(TimeSpan.Zero);
            _viewModel.ExpansionRate.ShouldBe(0);
            _viewModel.IsLoading.ShouldBe(false);
            _viewModel.CacheStatus.ShouldContain("Loading");
        }

        [Fact]
        public void IsLoading_InitiallyFalse()
        {
            // Assert
            _viewModel.IsLoading.ShouldBe(false);
        }

        [Fact]
        public void PropertyChange_UpdatesTotalMatches_WhenValueChanged()
        {
            // Arrange
            var initialValue = _viewModel.TotalMatches;

            // Act
            _viewModel.TotalMatches = 10;

            // Assert
            _viewModel.TotalMatches.ShouldNotBe(initialValue);
            _viewModel.TotalMatches.ShouldBe(10);
        }

        [Fact]
        public void PropertyChange_UpdatesWinRatePercentage_WhenValueChanged()
        {
            // Arrange
            var initialValue = _viewModel.WinRatePercentage;

            // Act
            _viewModel.WinRatePercentage = 75.5;

            // Assert
            _viewModel.WinRatePercentage.ShouldNotBe(initialValue);
            _viewModel.WinRatePercentage.ShouldBe(75.5);
        }

        [Fact]
        public void PropertyChange_UpdatesAverageDuration_WhenValueChanged()
        {
            // Arrange
            var initialValue = _viewModel.AverageDuration;
            var newDuration = TimeSpan.FromMinutes(30);

            // Act
            _viewModel.AverageDuration = newDuration;

            // Assert
            _viewModel.AverageDuration.ShouldNotBe(initialValue);
            _viewModel.AverageDuration.ShouldBe(newDuration);
        }

        [Fact]
        public void PropertyChange_UpdatesExpansionRate_WhenValueChanged()
        {
            // Arrange
            var initialValue = _viewModel.ExpansionRate;

            // Act
            _viewModel.ExpansionRate = 0.5;

            // Assert
            _viewModel.ExpansionRate.ShouldNotBe(initialValue);
            _viewModel.ExpansionRate.ShouldBe(0.5);
        }
    }
}
