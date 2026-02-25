using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MobileApp.Services;
using System.Text.Json;
using Microsoft.Maui.Controls;
using Microsoft.AspNetCore.SignalR.Client;
using Shared.Models;

namespace MobileApp.ViewModels
{
    public partial class DashboardPageViewModel : ObservableObject, IAsyncDisposable
    {
        private readonly IBotControlService _botControlService;
        private readonly HttpClient _httpClient;
        private HubConnection? _hubConnection;
        private const string ApiBaseUrl = "https://localhost:7138/api/bot/";
        private bool _disposed = false;

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
        private int marines = 0;

        [ObservableProperty]
        private int vultures = 0;

        [ObservableProperty]
        private int militaryCount = 0;

        [ObservableProperty]
        private bool hasExpanded = false;

        [ObservableProperty]
        private bool enemyScouted = false;

        [ObservableProperty]
        private string currentStrategy = "Default";

        [ObservableProperty]
        private string selectedStrategy = "Aggressive";

        [ObservableProperty]
        private List<string> availableStrategies = new() { "Aggressive", "Economic", "Defensive" };

        [ObservableProperty]
        private string confirmationMessage = "";

        [ObservableProperty]
        private bool showConfirmation = false;

        [ObservableProperty]
        private bool isLoading = false;

        public DashboardPageViewModel(IBotControlService botControlService, HttpClient httpClient)
        {
            _botControlService = botControlService;
            _httpClient = httpClient;

            // SignalR stuff
            // Initialize the Hub Connection
            _hubConnection = new HubConnectionBuilder()
                .WithUrl($"{ApiBaseUrl}gameHub")
                .WithAutomaticReconnect()
                .Build();
            
            // Attach SignalR listener for game state updates
            _hubConnection.On<GameStateDto>("ReceiveGameState", (state) =>
            {
                // supposedly MainThread is safer for UI
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    // Populate view model properties from GameStateDto
                    IsRunning = state.IsRunning;
                    InGame = state.InGame;
                    Workers = state.WorkerCount;
                    MilitaryCount = state.MilitaryCount;
                    Minerals = state.Minerals;
                    Gas = state.Gas;
                    Supply = state.SupplyUsed;
                    SupplyTotal = state.SupplyTotal;
                    HasExpanded = state.HasExpanded;
                    EnemyScouted = state.EnemyScouted;
                    CurrentStrategy = state.StrategyMode.ToString();
                });
            });

            // Attach SignalR listener for match end events
            _hubConnection.On<string>("MatchEnded", (result) =>
            {
                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    // Notify user that the match has ended
                    await Application.Current!.MainPage!.DisplayAlert("Match Ended", $"Match result: {result}", "OK");
                    
                    // Properly disconnect and dispose
                    await DisconnectAsync();
                });
            });

            // Handle reconnection
            _hubConnection.Reconnected += async (connectionId) =>
            {
                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    System.Diagnostics.Debug.WriteLine("SignalR reconnected");
                    await _hubConnection.InvokeAsync("SubscribeToUpdates");
                });
            };

            // Handle disconnection
            _hubConnection.Closed += async (exception) =>
            {
                if (exception != null)
                {
                    System.Diagnostics.Debug.WriteLine($"SignalR disconnected with error: {exception.Message}");
                }
            };

            // Start the connection
            Task.Run(async () =>
            {
                try
                {
                    await _hubConnection.StartAsync();
                    await _hubConnection.InvokeAsync("SubscribeToUpdates");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Failed to start SignalR connection: {ex.Message}");
                }
            });
        }

        [RelayCommand]
        public async Task RefreshGameState()
        {
            IsLoading = true;
            try
            {
                await FetchUnits();
            }
            catch (Exception ex)
            {
                await Application.Current!.MainPage!.DisplayAlert("Error", $"Failed to refresh: {ex.Message}", "OK");
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        public async Task Expand()
        {
            await _botControlService.ExpandAsync();
            await RefreshGameState();
        }

        [RelayCommand]
        public async Task ScoutMap()
        {
            await _botControlService.ScoutMapAsync();
            await RefreshGameState();
        }

        [RelayCommand]
        public async Task ToggleStrategy()
        {
            await _botControlService.ToggleStrategyAsync();
            await RefreshGameState();
        }

        [RelayCommand]
        public async Task SetStrategy()
        {
            if (string.IsNullOrEmpty(SelectedStrategy))
            {
                await Application.Current!.MainPage!.DisplayAlert("Error", "Please select a strategy", "OK");
                return;
            }

            bool success = await _botControlService.ChangeStrategyAsync(SelectedStrategy);
            if (success)
            {
                ConfirmationMessage = $"API set strategy to {SelectedStrategy}";
                ShowConfirmation = true;

                // Auto-hide confirmation after 3 seconds (non-blocking)
                _ = Task.Delay(3000).ContinueWith(_ => ShowConfirmation = false);
            }
            else
            {
                await Application.Current!.MainPage!.DisplayAlert("Error", $"Failed to set strategy to {SelectedStrategy}", "OK");
            }

            await RefreshGameState();
        }

        [RelayCommand]
        public async Task ToggleTheme()
        {
            Application.Current.UserAppTheme =
            Application.Current.UserAppTheme == AppTheme.Dark
                ? AppTheme.Light
                : AppTheme.Dark;
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

                    Marines = root.GetProperty("marines").GetInt32();
                    Vultures = root.GetProperty("vultures").GetInt32();
                }
            }
            catch { }
        }

        /// <summary>
        /// Properly disconnects from SignalR hub and cleans up resources
        /// </summary>
        private async Task DisconnectAsync()
        {
            if (_hubConnection != null)
            {
                try
                {
                    if (_hubConnection.State == HubConnectionState.Connected)
                    {
                        await _hubConnection.StopAsync();
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error stopping SignalR connection: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Disposes the SignalR connection and prevents memory leaks
        /// Called when the ViewModel is destroyed (page navigated away)
        /// </summary>
        public async ValueTask DisposeAsync()
        {
            if (_disposed)
                return;

            _disposed = true;

            if (_hubConnection != null)
            {
                try
                {
                    // Stop the connection gracefully
                    if (_hubConnection.State == HubConnectionState.Connected || 
                        _hubConnection.State == HubConnectionState.Connecting)
                    {
                        await _hubConnection.StopAsync();
                    }
                    
                    // Dispose the connection to release resources
                    await _hubConnection.DisposeAsync();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error disposing SignalR connection: {ex.Message}");
                }
                finally
                {
                    _hubConnection = null;
                }
            }
        }
    }
}
