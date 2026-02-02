using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MobileApp.Services;
using System.Text.Json;

namespace MobileApp.ViewModels
{
    public partial class HomePageViewModel : ObservableObject
    {
        private readonly IBotControlService _botControlService;
        private readonly HttpClient _httpClient;
        private const string ApiBaseUrl = "https://localhost:7138/api/bot/";

        [ObservableProperty]
        private string? statusMessage = "";

        [ObservableProperty]
        private bool isRunning = false;

        [ObservableProperty]
        private bool inGame = false;

        [ObservableProperty]
        private int supply = 0;

        [ObservableProperty]
        private int supplyTotal = 0;

        [ObservableProperty]
        private int minerals = 0;

        [ObservableProperty]
        private int gas = 0;

        [ObservableProperty]
        private int workers = 0;

        [ObservableProperty]
        private string? strategyName = "None";

        [ObservableProperty]
        private bool strategyPaused = false;

        [ObservableProperty]
        private int buildOrderIndex = 0;

        [ObservableProperty]
        private int buildOrderCount = 0;

        [ObservableProperty]
        private int playerBases = 0;

        [ObservableProperty]
        private int exploredBases = 0;

        [ObservableProperty]
        private int enemyBasesFound = 0;

        [ObservableProperty]
        private int totalPotentialBases = 0;

        [ObservableProperty]
        private int totalUnits = 0;

        [ObservableProperty]
        private int marines = 0;

        [ObservableProperty]
        private int vultures = 0;

        [ObservableProperty]
        private int wraiths = 0;

        [ObservableProperty]
        private int scvs = 0;

        [ObservableProperty]
        private bool isScouting = false;

        [ObservableProperty]
        private int pendingOrders = 0;

        [ObservableProperty]
        private bool hasWorkerAssigned = false;

        [ObservableProperty]
        private bool isLoading = false;

        public HomePageViewModel(IBotControlService botControlService, HttpClient httpClient)
        {
            _botControlService = botControlService;
            _httpClient = httpClient;
        }

        [RelayCommand]
        private async Task HelloWorld()
        {
            StatusMessage = await _botControlService.HelloWorldAsync();
        }

        [RelayCommand]
        private async Task BuildBunker()
        {
            await _botControlService.BuildBunkerAtChokepointAsync();
            await RefreshGameState();
        }

        [RelayCommand]
        private async Task BuildSupplyDepot()
        {
            await _botControlService.BuildSupplyDepotAtChokepointAsync();
            await RefreshGameState();
        }

        [RelayCommand]
        private async Task ToggleStrategy()
        {
            await _botControlService.ToggleStrategyAsync();
            await RefreshGameState();
        }

        [RelayCommand]
        private async Task ToggleAttackEnemyBase()
        {
            await _botControlService.ToggleAttackEnemyBaseAsync();
            await RefreshGameState();
        }

        [RelayCommand]
        private async Task ScoutMap()
        {
            await _botControlService.ScoutMapAsync();
            await RefreshGameState();
        }

        [RelayCommand]
        private async Task TogglePauseBot()
        {
            await _botControlService.TogglePauseBot();
            await RefreshGameState();
        }

        [RelayCommand]
        public async Task RefreshGameState()
        {
            IsLoading = true;
            try
            {
                await FetchGameStatus();
                await FetchStrategy();
                await FetchBases();
                await FetchUnits();
                await FetchConstruction();
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error refreshing game state: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task FetchGameStatus()
        {
            try
            {
                var response = await _httpClient.GetAsync($"{ApiBaseUrl}status");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    using var doc = JsonDocument.Parse(json);
                    var root = doc.RootElement;

                    IsRunning = root.GetProperty("isRunning").GetBoolean();
                    InGame = root.GetProperty("inGame").GetBoolean();
                    Supply = root.GetProperty("supply").GetInt32();
                    SupplyTotal = root.GetProperty("supplyTotal").GetInt32();
                    Minerals = root.GetProperty("minerals").GetInt32();
                    Gas = root.GetProperty("gas").GetInt32();
                    Workers = root.GetProperty("workers").GetInt32();
                }
            }
            catch { }
        }

        private async Task FetchStrategy()
        {
            try
            {
                var response = await _httpClient.GetAsync($"{ApiBaseUrl}strategy");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    using var doc = JsonDocument.Parse(json);
                    var root = doc.RootElement;

                    StrategyName = root.GetProperty("name").GetString();
                    StrategyPaused = root.GetProperty("isPaused").GetBoolean();
                    BuildOrderIndex = root.GetProperty("buildOrderIndex").GetInt32();
                    BuildOrderCount = root.GetProperty("buildOrderCount").GetInt32();
                }
            }
            catch { }
        }

        private async Task FetchBases()
        {
            try
            {
                var response = await _httpClient.GetAsync($"{ApiBaseUrl}bases");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    using var doc = JsonDocument.Parse(json);
                    var root = doc.RootElement;

                    PlayerBases = root.GetProperty("playerBases").GetInt32();
                    ExploredBases = root.GetProperty("exploredBases").GetInt32();
                    EnemyBasesFound = root.GetProperty("enemyBasesFound").GetInt32();
                    TotalPotentialBases = root.GetProperty("totalPotentialBases").GetInt32();
                }
            }
            catch { }
        }

        private async Task FetchUnits()
        {
            try
            {
                var response = await _httpClient.GetAsync($"{ApiBaseUrl}units");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    using var doc = JsonDocument.Parse(json);
                    var root = doc.RootElement;

                    TotalUnits = root.GetProperty("total").GetInt32();
                    Marines = root.GetProperty("marines").GetInt32();
                    Vultures = root.GetProperty("vultures").GetInt32();
                    Wraiths = root.GetProperty("wraiths").GetInt32();
                    Scvs = root.GetProperty("sCVs").GetInt32();
                    IsScouting = root.GetProperty("isScouting").GetBoolean();
                }
            }
            catch { }
        }

        private async Task FetchConstruction()
        {
            try
            {
                var response = await _httpClient.GetAsync($"{ApiBaseUrl}construction");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    using var doc = JsonDocument.Parse(json);
                    var root = doc.RootElement;

                    PendingOrders = root.GetProperty("pendingOrders").GetInt32();
                    HasWorkerAssigned = root.GetProperty("hasWorkerAssigned").GetBoolean();
                }
            }
            catch { }
        }
    }
}
