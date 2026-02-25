using System.Text.Json;
using System.Diagnostics;

namespace MobileApp.Services
{
    public interface ICacheService
    {
        Task SaveMatchesAsync(List<MatchCacheDto> matches);
        Task<List<MatchCacheDto>> GetCachedMatchesAsync();
        Task ClearCacheAsync();
    }

    public class MatchCacheDto
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

    public class CacheService : ICacheService
    {
        private const string CacheKey = "matches_cache";
        private const string CacheTimestampKey = "matches_cache_timestamp";

        public async Task SaveMatchesAsync(List<MatchCacheDto> matches)
        {
            try
            {
                var json = JsonSerializer.Serialize(matches);
                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    Preferences.Set(CacheKey, json);
                    Preferences.Set(CacheTimestampKey, DateTime.UtcNow.Ticks);
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Cache save error: {ex.Message}");
            }
        }

        public async Task<List<MatchCacheDto>> GetCachedMatchesAsync()
        {
            try
            {
                return await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    if (Preferences.ContainsKey(CacheKey))
                    {
                        var json = Preferences.Get(CacheKey, "");
                        if (!string.IsNullOrEmpty(json))
                        {
                            return JsonSerializer.Deserialize<List<MatchCacheDto>>(json) ?? new();
                        }
                    }
                    return new List<MatchCacheDto>();
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Cache read error: {ex.Message}");
                return new List<MatchCacheDto>();
            }
        }

        public async Task ClearCacheAsync()
        {
            try
            {
                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    Preferences.Remove(CacheKey);
                    Preferences.Remove(CacheTimestampKey);
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Cache clear error: {ex.Message}");
            }
        }

        public static TimeSpan GetCacheAge()
        {
            if (Preferences.ContainsKey(CacheTimestampKey))
            {
                var timestamp = Preferences.Get(CacheTimestampKey, 0L);
                var cacheTime = new DateTime(timestamp);
                return DateTime.UtcNow - cacheTime;
            }
            return TimeSpan.MaxValue;
        }
    }
}
