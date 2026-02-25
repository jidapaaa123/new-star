using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Text.Json;
using Microsoft.Maui.Controls;
using MobileApp.Services;

namespace MobileApp.ViewModels
{
    public class MatchDto
    {
        public int Id { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public string Result { get; set; } = "";
        public int FinalWorkerCount { get; set; }
        public int FinalMinerals { get; set; }
        public int FinalGas { get; set; }
        public int FinalMilitaryCount { get; set; }

        public TimeSpan Duration => EndTime.HasValue ? EndTime.Value - StartTime : TimeSpan.Zero;
    }

    public partial class HistoryPageViewModel : ObservableObject
    {
        private readonly HttpClient _httpClient;
        private readonly ICacheService _cacheService;
        private const string ApiBaseUrl = "https://localhost:7138/api/matches/";

        [ObservableProperty]
        private ObservableCollection<MatchDto> matches = new();

        [ObservableProperty]
        private bool isLoading = false;

        [ObservableProperty]
        private string cacheStatus = "Loading from cache...";

        public HistoryPageViewModel(HttpClient httpClient, ICacheService cacheService)
        {
            _httpClient = httpClient;
            _cacheService = cacheService;
        }

        [RelayCommand]
        public async Task LoadMatches()
        {
            IsLoading = true;
            try
            {
                // 1. Load from LOCAL cache first (instant display)
                var cachedMatches = await _cacheService.GetCachedMatchesAsync();
                System.Diagnostics.Debug.WriteLine($"Loaded {cachedMatches.Count} matches from cache");
                if (cachedMatches.Count > 0)
                {
                    Matches.Clear();
                    foreach (var match in cachedMatches)
                    {
                        Matches.Add(new MatchDto
                        {
                            Id = match.Id,
                            StartTime = match.StartTime,
                            EndTime = match.EndTime,
                            Result = match.Result,
                            FinalWorkerCount = match.FinalWorkerCount,
                            FinalMinerals = match.FinalMinerals,
                            FinalGas = match.FinalGas,
                            FinalMilitaryCount = match.FinalMilitaryCount
                        });
                    }
                    CacheStatus = "Showing cached data - updating from server...";
                }
                else
                {
                    CacheStatus = "No cached data - fetching from server...";
                }

                // 2. Fetch fresh data from API in background
                try
                {
                    var response = await _httpClient.GetAsync(ApiBaseUrl);
                    if (response.IsSuccessStatusCode)
                    {
                        var json = await response.Content.ReadAsStringAsync();
                        System.Diagnostics.Debug.WriteLine($"API Response: {json}");
                        
                        // Use proper JSON deserialization with case-insensitive property matching
                        var options = new JsonSerializerOptions 
                        { 
                            PropertyNameCaseInsensitive = true 
                        };
                        var freshMatches = JsonSerializer.Deserialize<List<MatchCacheDto>>(json, options) ?? new();
                        System.Diagnostics.Debug.WriteLine($"Deserialized {freshMatches.Count} matches");
                        
                        foreach (var match in freshMatches)
                        {
                            System.Diagnostics.Debug.WriteLine($"Match: Id={match.Id}, Result={match.Result}, StartTime={match.StartTime}");
                        }

                        // 3. Save to LOCAL cache
                        await _cacheService.SaveMatchesAsync(freshMatches);

                        // 4. Update UI with fresh data
                        Matches.Clear();
                        foreach (var match in freshMatches)
                        {
                            Matches.Add(new MatchDto
                            {
                                Id = match.Id,
                                StartTime = match.StartTime,
                                EndTime = match.EndTime,
                                Result = match.Result,
                                FinalWorkerCount = match.FinalWorkerCount,
                                FinalMinerals = match.FinalMinerals,
                                FinalGas = match.FinalGas,
                                FinalMilitaryCount = match.FinalMilitaryCount
                            });
                        }
                        System.Diagnostics.Debug.WriteLine($"UI updated with {Matches.Count} matches");
                        CacheStatus = "Updated from server ?";
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"API returned: {response.StatusCode}");
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"API Error: {ex.Message}");
                    // Network error - but we still have cached data!
                    if (cachedMatches.Count > 0)
                    {
                        CacheStatus = "Server unavailable - showing cached data";
                    }
                    else
                    {
                        CacheStatus = "No connection and no cached data";
                        await Application.Current!.MainPage!.DisplayAlert("Error", $"Failed to load matches: {ex.Message}", "OK");
                    }
                }
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        public async Task MatchSelected(MatchDto match)
        {
            if (match == null) return;
            
            await Application.Current!.MainPage!.DisplayAlert(
                "Match Details",
                $"Result: {match.Result}\n" +
                $"Duration: {match.Duration:hh\\:mm\\:ss}\n" +
                $"Final Minerals: {match.FinalMinerals}\n" +
                $"Final Gas: {match.FinalGas}\n" +
                $"Workers: {match.FinalWorkerCount}",
                "OK"
            );
        }
    }
}
