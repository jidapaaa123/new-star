using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Text.Json;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Layouts;
using MobileApp.Services;

namespace MobileApp.ViewModels
{
    public partial class AnalyticsPageViewModel : ObservableObject
    {
        private readonly HttpClient _httpClient;
        private readonly ICacheService _cacheService;
        private const string ApiBaseUrl = "https://localhost:7138/api/matches/";

        [ObservableProperty]
        private int totalMatches = 0;

        [ObservableProperty]
        private double winRatePercentage = 0;

        [ObservableProperty]
        private TimeSpan averageDuration = TimeSpan.Zero;

        [ObservableProperty]
        private double expansionRate = 0;

        [ObservableProperty]
        private bool isLoading = false;

        [ObservableProperty]
        private string cacheStatus = "Loading...";

        private List<MatchCacheDto> _allMatches = new();

        public AnalyticsPageViewModel(HttpClient httpClient, ICacheService cacheService)
        {
            _httpClient = httpClient;
            _cacheService = cacheService;
        }

        [RelayCommand]
        public async Task LoadAnalytics()
        {
            IsLoading = true;
            try
            {
                // 1. Load from LOCAL cache first
                var cachedMatches = await _cacheService.GetCachedMatchesAsync();
                if (cachedMatches.Count > 0)
                {
                    _allMatches = cachedMatches;
                    CalculateStatistics();
                    CacheStatus = "Showing cached data - updating from server...";
                }
                else
                {
                    CacheStatus = "No cached data - fetching from server...";
                }

                // 2. Fetch fresh data from API
                try
                {
                    var response = await _httpClient.GetAsync(ApiBaseUrl);
                    if (response.IsSuccessStatusCode)
                    {
                        var json = await response.Content.ReadAsStringAsync();
                        var matchesArray = JsonDocument.Parse(json).RootElement.EnumerateArray().ToList();
                        
                        var freshMatches = new List<MatchCacheDto>();
                        foreach (var match in matchesArray)
                        {
                            freshMatches.Add(new MatchCacheDto
                            {
                                Id = match.GetProperty("id").GetInt32(),
                                StartTime = match.GetProperty("startTime").GetDateTime(),
                                EndTime = match.TryGetProperty("endTime", out var endTime) ? endTime.GetDateTime() : null,
                                Result = match.GetProperty("result").GetString() ?? "",
                                FinalWorkerCount = match.GetProperty("finalWorkerCount").GetInt32(),
                                FinalMinerals = match.GetProperty("finalMinerals").GetInt32(),
                                FinalGas = match.GetProperty("finalGas").GetInt32(),
                                FinalMilitaryCount = match.GetProperty("finalMilitaryCount").GetInt32()
                            });
                        }

                        // 3. Save to cache
                        await _cacheService.SaveMatchesAsync(freshMatches);

                        // 4. Update with fresh data
                        _allMatches = freshMatches;
                        CalculateStatistics();
                        CacheStatus = "Updated from server ?";
                    }
                }
                catch (Exception ex)
                {
                    // Network error - but we still have cached data!
                    if (cachedMatches.Count > 0)
                    {
                        CacheStatus = "Server unavailable - showing cached data";
                    }
                    else
                    {
                        CacheStatus = "No connection and no cached data";
                        await Application.Current!.MainPage!.DisplayAlert("Error", $"Failed to load analytics: {ex.Message}", "OK");
                    }
                }
            }
            finally
            {
                IsLoading = false;
            }
        }

        public async Task RenderChartsAsync(Grid winLossContainer, Grid workerContainer)
        {
            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                RenderWinLossChart(winLossContainer);
                RenderWorkerTrendChart(workerContainer);
            });
        }

        private void CalculateStatistics()
        {
            if (_allMatches.Count == 0)
                return;

            TotalMatches = _allMatches.Count;
            
            var wins = _allMatches.Count(m => m.Result == "Win");
            WinRatePercentage = TotalMatches > 0 ? (double)wins / TotalMatches * 100 : 0;

            var completedMatches = _allMatches.Where(m => m.EndTime.HasValue).ToList();
            if (completedMatches.Count > 0)
            {
                var totalDuration = completedMatches.Sum(m => (m.EndTime!.Value - m.StartTime).TotalSeconds);
                AverageDuration = TimeSpan.FromSeconds(totalDuration / completedMatches.Count);
            }

            var expandedMatches = _allMatches.Count(m => m.FinalWorkerCount > 15); // Heuristic
            ExpansionRate = TotalMatches > 0 ? (double)expandedMatches / TotalMatches : 0;
        }

        private void RenderWinLossChart(Grid container)
        {
            container.Children.Clear();

            var wins = _allMatches.Count(m => m.Result == "Win");
            var losses = _allMatches.Count(m => m.Result == "Loss");

            var verticalStack = new VerticalStackLayout { Spacing = 12, Padding = 12 };

            var winBar = new HorizontalStackLayout { Spacing = 8 };
            winBar.Add(new Label { Text = $"Wins: {wins}", WidthRequest = 80 });
            winBar.Add(new BoxView { Color = Colors.Green, WidthRequest = wins * 5, HeightRequest = 30 });
            verticalStack.Add(winBar);

            var lossBar = new HorizontalStackLayout { Spacing = 8 };
            lossBar.Add(new Label { Text = $"Losses: {losses}", WidthRequest = 80 });
            lossBar.Add(new BoxView { Color = Colors.Red, WidthRequest = losses * 5, HeightRequest = 30 });
            verticalStack.Add(lossBar);

            container.Add(verticalStack);
        }

        private void RenderWorkerTrendChart(Grid container)
        {
            container.Children.Clear();

            var sortedMatches = _allMatches.OrderBy(m => m.StartTime).ToList();
            
            var verticalStack = new VerticalStackLayout { Spacing = 12, Padding = 12 };
            verticalStack.Add(new Label 
            { 
                Text = $"Average Workers: {(sortedMatches.Count > 0 ? sortedMatches.Average(m => m.FinalWorkerCount) : 0):F1}",
                FontSize = 16,
                FontAttributes = FontAttributes.Bold
            });

            var horizontalStack = new HorizontalStackLayout { Spacing = 4, Padding = 8 };
            foreach (var match in sortedMatches.TakeLast(10))
            {
                var bar = new BoxView 
                { 
                    Color = Colors.Blue, 
                    WidthRequest = 15,
                    HeightRequest = match.FinalWorkerCount * 2
                };
                horizontalStack.Add(bar);
            }

            verticalStack.Add(horizontalStack);
            container.Add(verticalStack);
        }
    }
}
