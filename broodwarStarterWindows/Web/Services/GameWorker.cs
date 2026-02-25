using Microsoft.AspNetCore.SignalR;
using Shared;
using Shared.Models;
using Web.Hubs;

namespace Web.Services;

public class GameWorker : BackgroundService
{
    private readonly IHubContext<GameStateHub> _hubContext;
    private readonly ILogger<GameWorker> _logger;
    private readonly MyStarcraftBot _bot;
    private bool _wasInGame = false;

    public GameWorker(IHubContext<GameStateHub> hubContext, MyStarcraftBot bot, ILogger<GameWorker> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
        _bot = bot;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("Broadcasting game state at: {time}", DateTimeOffset.Now);

            // Get actual game state from the bot
            var currentState = _bot.GetCurrentGameState();

            // Check if match just ended (transition from InGame to not InGame)
            if (_wasInGame && !currentState.InGame)
            {
                _logger.LogInformation("Match has ended, broadcasting match end event");
                // Determine the result based on Match data
                var result = _bot.Match?.Result ?? "Unknown";
                await _hubContext.Clients.Group("GameUpdates")
                    .SendAsync("MatchEnded", result);
            }

            _wasInGame = currentState.InGame;

            // Push to the "GameUpdates" group defined in your Hub
            await _hubContext.Clients.Group("GameUpdates")
                .SendAsync("ReceiveGameState", currentState, stoppingToken);

            // Wait for 2-3 seconds before the next update (only if not cancelled)
            if (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);
            }
        }
    }
}