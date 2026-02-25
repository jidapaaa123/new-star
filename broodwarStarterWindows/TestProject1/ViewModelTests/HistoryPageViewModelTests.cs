using Moq;
using MobileApp.Services;
using MobileApp.ViewModels;
using Shouldly;
using System.Collections.ObjectModel;

namespace TestProject1.ViewModelTests
{
    public class HistoryPageViewModelTests
    {
        private readonly Mock<HttpClient> _mockHttpClient;
        private readonly Mock<ICacheService> _mockCacheService;
        private readonly HistoryPageViewModel _viewModel;

        public HistoryPageViewModelTests()
        {
            _mockHttpClient = new Mock<HttpClient>();
            _mockCacheService = new Mock<ICacheService>();
            var httpClient = new HttpClient(new HttpClientHandler { UseProxy = false })
            {
                Timeout = TimeSpan.FromMilliseconds(1)
            };
            _viewModel = new HistoryPageViewModel(httpClient, _mockCacheService.Object);
        }

        [Fact]
        public void Constructor_InitializesPropertiesWithDefaultValues()
        {
            // Assert
            _viewModel.IsLoading.ShouldBe(false);
            _viewModel.Matches.ShouldBeEmpty();
            _viewModel.CacheStatus.ShouldContain("Loading");
        }

        [Fact]
        public async Task LoadMatchesCommand_PopulatesMatches_WhenCachedDataExists()
        {
            // Arrange
            var cachedMatches = new List<MatchCacheDto>
            {
                new() { Id = 1, StartTime = DateTime.UtcNow.AddHours(-1), Result = "Win", FinalMinerals = 500 },
                new() { Id = 2, StartTime = DateTime.UtcNow.AddHours(-2), Result = "Loss", FinalMinerals = 300 }
            };

            _mockCacheService.Setup(s => s.GetCachedMatchesAsync())
                .ReturnsAsync(cachedMatches);

            // Act
            await _viewModel.LoadMatchesCommand.ExecuteAsync(null);

            // Assert
            _viewModel.Matches.Count.ShouldBe(2);
            _viewModel.Matches[0].Id.ShouldBe(1);
            _viewModel.Matches[0].Result.ShouldBe("Win");
        }
    }
}
