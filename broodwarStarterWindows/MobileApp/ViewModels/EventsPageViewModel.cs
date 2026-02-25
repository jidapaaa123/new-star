using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MobileApp.Services;

namespace MobileApp.ViewModels
{
    public partial class EventsPageViewModel : ObservableObject
    {
        private readonly IBotControlService _botControlService;
        private int _currentMatchId = 1; // Default to match 1, can be set from parent

        [ObservableProperty]
        private bool isLoading = false;

        [ObservableProperty]
        private List<GameEventItem> events = new();

        [ObservableProperty]
        private bool hasNoEvents = true;

        public EventsPageViewModel(IBotControlService botControlService)
        {
            _botControlService = botControlService;
        }

        public void SetMatchId(int matchId)
        {
            _currentMatchId = matchId;
        }

        [RelayCommand]
        public async Task RefreshEvents()
        {
            IsLoading = true;
            try
            {
                System.Diagnostics.Debug.WriteLine("RefreshEvents called");
                // Get the latest match ID
                var latestMatchId = await _botControlService.GetLatestMatchIdAsync();
                System.Diagnostics.Debug.WriteLine($"Latest match ID: {latestMatchId}");
                
                if (latestMatchId.HasValue)
                {
                    _currentMatchId = latestMatchId.Value;
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"No latest match found, using default: {_currentMatchId}");
                }

                var gameEvents = await _botControlService.GetMatchEventsAsync(_currentMatchId);
                System.Diagnostics.Debug.WriteLine($"Retrieved events: {gameEvents?.Count ?? 0}");

                if (gameEvents != null && gameEvents.Count > 0)
                {
                    // Convert to UI items with color coding
                    Events = gameEvents
                        .OrderByDescending(e => e.Timestamp)
                        .Select(e => new GameEventItem
                        {
                            EventType = e.EventType,
                            Description = e.Description,
                            Timestamp = e.Timestamp,
                            EventColor = GetEventColor(e.EventType),
                            FormattedTime = e.Timestamp.ToString("HH:mm:ss")
                        })
                        .ToList();

                    HasNoEvents = false;
                    System.Diagnostics.Debug.WriteLine($"Events displayed: {Events.Count}");
                }
                else
                {
                    Events.Clear();
                    HasNoEvents = true;
                    System.Diagnostics.Debug.WriteLine("No events found");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"RefreshEvents exception: {ex}");
                await Application.Current!.MainPage!.DisplayAlert("Error", $"Failed to load events: {ex.Message}", "OK");
                HasNoEvents = true;
            }
            finally
            {
                IsLoading = false;
            }
        }

        private static Color GetEventColor(string eventType) => eventType switch
        {
            "expansion" => Colors.Green,
            "upgrade" => Colors.Blue,
            "scout" => Colors.Orange,
            "attack" => Colors.Red,
            "supply_blocked" => Colors.Purple,
            _ => Colors.Gray
        };
    }

    public class GameEventItem
    {
        public string EventType { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public Color EventColor { get; set; }
        public string FormattedTime { get; set; } = string.Empty;
    }
}
