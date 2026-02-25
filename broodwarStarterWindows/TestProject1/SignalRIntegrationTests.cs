using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Shared;
using Shared.Data;
using Shared.Interfaces;
using Shared.Models;
using Shared.Services;
using Shouldly;
using Web.Hubs;
using Web.Services;
using Xunit;

namespace TestProject1;

/// <summary>
/// SignalR integration tests using a real Kestrel server on a dynamic port.
/// This fixture creates an actual HTTP server that listens on localhost with a dynamically assigned port,
/// allowing real WebSocket connections for SignalR testing.
/// 
/// NOTE: The GameWorker background service is cancelled during disposal, which is EXPECTED and NORMAL.
/// This indicates proper cleanup of the service. The improved GameWorker.cs now handles cancellation
/// gracefully without throwing exceptions.
/// </summary>
public class RealServerSignalRTestFixture : IAsyncLifetime
{
    private WebApplication? _app;
    private CancellationTokenSource? _cts;
    public HubConnection HubConnection { get; private set; } = null!;
    public string ServerUrl { get; private set; } = string.Empty;

    public async Task InitializeAsync()
    {
        // Create a real web application with Kestrel server
        var builder = WebApplication.CreateBuilder();
        
        // Configure services  
        builder.Services.AddSingleton<MyStarcraftBot>();
        builder.Services.AddSingleton<StarCraftService>();
        builder.Services.AddSingleton<UserPreferencesService>();
        builder.Services.AddSingleton<IMatchRepository, MatchRepository>();
        builder.Services.AddSingleton<IGameEventRepository, GameEventRepository>();
        builder.Services.AddSignalR();
        builder.Services.AddHostedService<GameWorker>();
        builder.Services.AddControllers();
        
        // Add Entity Framework Core with SQLite in-memory database for testing
        builder.Services.AddDbContextFactory<MatchContext>(options =>
            options.UseSqlite("Data Source=:memory:"), ServiceLifetime.Singleton);

        // Use a specific port for testing (port 0 = OS assigns available port)
        builder.WebHost.UseUrls("http://127.0.0.1:0");

        _app = builder.Build();

        // Map SignalR hub
        _app.MapHub<GameStateHub>("api/bot/gameHub");
        _app.MapControllers();

        // Start the application
        _cts = new CancellationTokenSource();
        var startTask = _app.StartAsync(_cts.Token);

        // Wait for the app to fully start
        await startTask.ConfigureAwait(false);
        
        // Give it a moment to bind to the port
        await Task.Delay(500);
        
        // Get the actual server address - access through the IServer directly after starting
        try
        {
            var server = _app.Services.GetRequiredService<Microsoft.AspNetCore.Hosting.Server.IServer>();
            var addressFeature = server.Features.Get<Microsoft.AspNetCore.Hosting.Server.Features.IServerAddressesFeature>();
            
            if (addressFeature?.Addresses.Count > 0)
            {
                ServerUrl = addressFeature.Addresses.First();
            }
            else
            {
                throw new InvalidOperationException("No server addresses available");
            }
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to get server address: {ex.Message}", ex);
        }

        // Create SignalR connection to the real server
        HubConnection = new HubConnectionBuilder()
            .WithUrl($"{ServerUrl}/api/bot/gameHub")
            .WithAutomaticReconnect()
            .Build();

        // Connect
        await HubConnection.StartAsync();
    }

    public async Task DisposeAsync()
    {
        try
        {
            if (HubConnection != null)
            {
                if (HubConnection.State == HubConnectionState.Connected || HubConnection.State == HubConnectionState.Connecting)
                {
                    await HubConnection.StopAsync();
                }
                await HubConnection.DisposeAsync();
            }
        }
        catch { }

        try
        {
            if (_cts != null)
            {
                // Cancelling the token will trigger graceful shutdown in background services
                // This is EXPECTED and indicates proper cleanup
                _cts.Cancel();
                _cts.Dispose();
            }
        }
        catch { }

        try
        {
            if (_app != null)
            {
                await _app.DisposeAsync();
            }
        }
        catch { }
    }
}

public class SignalRIntegrationTests : IClassFixture<RealServerSignalRTestFixture>
{
    private readonly RealServerSignalRTestFixture _fixture;

    public SignalRIntegrationTests(RealServerSignalRTestFixture fixture)
    {
        _fixture = fixture;
    }

    /// <summary>
    /// Verifies that a real bidirectional SignalR WebSocket connection can be established.
    /// 
    /// ASSERTIONS:
    /// - Connection state is Connected after fixture initialization
    /// - Can invoke a hub method (Echo) and receive a response
    /// - Proves real network communication is working
    /// </summary>
    [Fact]
    public async Task SignalRHub_Should_Connect_Successfully()
    {
        // Assert - Connection is established during fixture initialization
        _fixture.HubConnection.State.ShouldBe(HubConnectionState.Connected);
        
        // Assert - Verify the connection is ACTUALLY working by invoking a method
        // This proves we have a real bidirectional connection before cleanup
        var response = await _fixture.HubConnection.InvokeAsync<string>("Echo", "test-message");
        response.ShouldNotBeNull();
        response.ShouldContain("Echo:");
    }

    /// <summary>
    /// Verifies that a client can subscribe to the GameUpdates broadcast group and receive real data.
    /// 
    /// ASSERTIONS:
    /// - Connection state is Connected before subscribing
    /// - Can subscribe to the GameUpdates group via hub method invocation
    /// - Connection remains Connected after subscription
    /// - Actually receives broadcast messages from the GameWorker background service
    /// - Received GameStateDto is valid and not null
    /// </summary>
    [Fact]
    public async Task SignalRHub_Should_Subscribe_To_GameUpdates_Group()
    {
        // Arrange - Verify connection is active BEFORE subscribing
        _fixture.HubConnection.State.ShouldBe(HubConnectionState.Connected);
        
        var messageReceived = new TaskCompletionSource<GameStateDto>();
        _fixture.HubConnection.On<GameStateDto>("ReceiveGameState", state =>
        {
            messageReceived.TrySetResult(state);
        });

        // Act - Subscribe to updates
        await _fixture.HubConnection.InvokeAsync("SubscribeToUpdates");

        // Assert - Connection still active after subscription
        _fixture.HubConnection.State.ShouldBe(HubConnectionState.Connected);
        
        // Assert - Wait for at least one message to be received (with timeout)
        // This proves the hub is actively broadcasting and we received it
        var receivedTask = await Task.WhenAny(
            messageReceived.Task,
            Task.Delay(TimeSpan.FromSeconds(5))
        );
        
        // Verify we actually received a message, not just timed out
        receivedTask.ShouldBe(messageReceived.Task);
        messageReceived.Task.IsCompletedSuccessfully.ShouldBeTrue();
        messageReceived.Task.Result.ShouldNotBeNull();
    }
}
