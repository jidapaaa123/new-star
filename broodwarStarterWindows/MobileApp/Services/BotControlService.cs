using System.Text.Json;

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
    }
}