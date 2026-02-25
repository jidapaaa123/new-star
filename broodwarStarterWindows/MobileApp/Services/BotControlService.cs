using System.Text.Json;
using System.Text.Json.Serialization;

namespace MobileApp.Services
{
    public class BotControlService : IBotControlService
    {
        private readonly HttpClient _httpClient;
        private const string ApiBaseUrl = "https://localhost:7138/api/bot/";

        public BotControlService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }
        public async Task<string?> HelloWorldAsync()
        {
            var response = await _httpClient.GetAsync($"{ApiBaseUrl}");
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadAsStringAsync();
            }

            return null;
        }

        public async Task<bool> BuildBunkerAtChokepointAsync()
        {
            var response = await _httpClient.PostAsync($"{ApiBaseUrl}chokebunker", null);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> BuildSupplyDepotAtChokepointAsync()
        {
            var response = await _httpClient.PostAsync($"{ApiBaseUrl}chokedepot", null);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> ToggleStrategyAsync()
        {
            var response = await _httpClient.PostAsync($"{ApiBaseUrl}togglestrat", null);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> ChangeStrategyAsync(string strategy)
        {
            var request = new { strategy = strategy };
            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
            var response = await _httpClient.PutAsync($"{ApiBaseUrl}strategy", content);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> ToggleAttackEnemyBaseAsync()
        {
            var response = await _httpClient.PostAsync($"{ApiBaseUrl}toggleattackenemybase", null);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> ScoutMapAsync()
        {
            var response = await _httpClient.PostAsync($"{ApiBaseUrl}scoutmap", null);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> TogglePauseBot()
        {
            var response = await _httpClient.PostAsync($"{ApiBaseUrl}togglepausebot", null);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> ExpandAsync()
        {
            var response = await _httpClient.PostAsync($"{ApiBaseUrl}expand", null);
            return response.IsSuccessStatusCode;
        }

        public async Task<int?> GetLatestMatchIdAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync($"{ApiBaseUrl}matches/latest");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    System.Diagnostics.Debug.WriteLine($"Latest Match Response: {json}");
                    using var doc = JsonDocument.Parse(json);
                    var root = doc.RootElement;
                    
                    // Try to get the id property (case-insensitive due to camelCase)
                    if (root.TryGetProperty("id", out var idElement))
                    {
                        if (idElement.TryGetInt32(out var id))
                        {
                            System.Diagnostics.Debug.WriteLine($"Latest Match ID: {id}");
                            return id;
                        }
                    }
                    else if (root.TryGetProperty("Id", out var idElement2))
                    {
                        if (idElement2.TryGetInt32(out var id))
                        {
                            System.Diagnostics.Debug.WriteLine($"Latest Match ID: {id}");
                            return id;
                        }
                    }
                    System.Diagnostics.Debug.WriteLine("Could not find id property in response");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"GetLatestMatchIdAsync failed: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetLatestMatchIdAsync exception: {ex}");
            }

            return null;
        }

        public async Task<List<(string EventType, string Description, DateTime Timestamp)>?> GetMatchEventsAsync(int matchId)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"GetMatchEventsAsync called with matchId: {matchId}");
                var response = await _httpClient.GetAsync($"{ApiBaseUrl}matches/{matchId}/events");
                System.Diagnostics.Debug.WriteLine($"GetMatchEventsAsync response: {response.StatusCode}");
                
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    System.Diagnostics.Debug.WriteLine($"Match Events Response: {json}");
                    using var doc = JsonDocument.Parse(json);
                    var root = doc.RootElement;

                    var events = new List<(string, string, DateTime)>();
                    foreach (var element in root.EnumerateArray())
                    {
                        var eventType = element.GetProperty("eventType").GetString() ?? "";
                        var description = element.GetProperty("description").GetString() ?? "";
                        var timestamp = element.GetProperty("timestamp").GetDateTime();

                        events.Add((eventType, description, timestamp));
                    }
                    System.Diagnostics.Debug.WriteLine($"Parsed {events.Count} events");
                    return events;
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"GetMatchEventsAsync failed: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetMatchEventsAsync exception: {ex}");
            }

            return null;
        }
    }
}