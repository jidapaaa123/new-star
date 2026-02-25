using Microsoft.AspNetCore.SignalR;
using Shared.Models;
using Web.Controllers;

namespace Web.Hubs
{
    public class GameStateHub : Hub
    {
        // The MAUI app calls this once to join the "room"
        public async Task SubscribeToUpdates()
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, "GameUpdates");
        }

        // This is the method the Background Service will trigger
        // It sends the state to everyone in the "GameUpdates" group
        public async Task BroadcastGameState(GameStateDto state)
        {
            await Clients.Group("GameUpdates").SendAsync("ReceiveGameState", state);
        }

        // This method broadcasts match end events to all connected clients
        public async Task BroadcastMatchEnd(string result)
        {
            await Clients.Group("GameUpdates").SendAsync("MatchEnded", result);
        }
        
        // Echo method for testing - allows clients to verify real bidirectional connection
        public string Echo(string message)
        {
            return $"Echo: {message}";
        }
    }
}
