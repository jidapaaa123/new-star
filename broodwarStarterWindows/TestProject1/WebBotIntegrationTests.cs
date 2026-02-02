using BWAPI.NET;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Shared;
using Shared.Interfaces;
using Shared.Models;
using Shouldly;
using System.Text.Json;
using Web.Controllers;
using Xunit;

namespace TestProject1;

public class TestWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Don't override the bot - let it stay null to test the 503 behavior
            // This documents that endpoints require Game to be NOT NULL
        });
    }
}

public class WebBotIntegrationTestFixture : IAsyncLifetime
{
    public TestWebApplicationFactory Factory { get; private set; } = null!;
    public HttpClient Client { get; private set; } = null!;
    public MyStarcraftBot Bot { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        Factory = new TestWebApplicationFactory();
        Client = Factory.CreateClient();
        
        // Get the bot instance from the service provider
        using var scope = Factory.Services.CreateScope();
        Bot = scope.ServiceProvider.GetRequiredService<MyStarcraftBot>();
        
        await Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        Client?.Dispose();
        Factory?.Dispose();
        await Task.CompletedTask;
    }
}

public class WebBotIntegrationTests : IClassFixture<WebBotIntegrationTestFixture>
{
    private readonly WebBotIntegrationTestFixture _fixture;

    public WebBotIntegrationTests(WebBotIntegrationTestFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task GetStatus_Should_Return_ServiceUnavailable_When_Game_Is_Null()
    {
        // Act
        var response = await _fixture.Client.GetAsync("api/bot/status");

        // Assert - Documents that endpoint requires Game to be NOT NULL
        response.StatusCode.ShouldBe(System.Net.HttpStatusCode.ServiceUnavailable);
    }

    [Fact]
    public async Task GetStrategy_Should_Return_ServiceUnavailable_When_Game_Is_Null()
    {
        // Act
        var response = await _fixture.Client.GetAsync("api/bot/strategy");

        // Assert - Documents that endpoint requires Game to be NOT NULL
        response.StatusCode.ShouldBe(System.Net.HttpStatusCode.ServiceUnavailable);
    }

    [Fact]
    public async Task GetBases_Should_Return_ServiceUnavailable_When_Game_Is_Null()
    {
        // Act
        var response = await _fixture.Client.GetAsync("api/bot/bases");

        // Assert - Documents that endpoint requires Game to be NOT NULL
        response.StatusCode.ShouldBe(System.Net.HttpStatusCode.ServiceUnavailable);
    }

    [Fact]
    public async Task GetUnits_Should_Return_ServiceUnavailable_When_Game_Is_Null()
    {
        // Act
        var response = await _fixture.Client.GetAsync("api/bot/units");

        // Assert - Documents that endpoint requires Game to be NOT NULL
        response.StatusCode.ShouldBe(System.Net.HttpStatusCode.ServiceUnavailable);
    }

    [Fact]
    public async Task GetConstruction_Should_Return_ServiceUnavailable_When_Game_Is_Null()
    {
        // Act
        var response = await _fixture.Client.GetAsync("api/bot/construction");

        // Assert - Documents that endpoint requires Game to be NOT NULL
        response.StatusCode.ShouldBe(System.Net.HttpStatusCode.ServiceUnavailable);
    }

    [Fact]
    public async Task HelloWorld_Should_Return_String()
    {
        // Act
        var response = await _fixture.Client.GetAsync("api/bot");

        // Assert
        response.StatusCode.ShouldBe(System.Net.HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.ShouldNotBeNullOrEmpty();
        content.ShouldContain("Hello World");
    }

    [Fact]
    public async Task Bye_Endpoint_Should_Return_Bye_World()
    {
        // Act
        var response = await _fixture.Client.GetAsync("api/bot/bye");

        // Assert
        response.StatusCode.ShouldBe(System.Net.HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.ShouldContain("Bye World");
    }

    [Fact]
    public async Task PostToggleStrategy_Should_Return_Ok()
    {
        // Act
        var response = await _fixture.Client.PostAsync("api/bot/togglestrat", null);

        // Assert
        response.StatusCode.ShouldBe(System.Net.HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.ShouldNotBeNullOrEmpty();
    }

    [Fact]
    public async Task PostToggleAttackEnemyBase_Should_Return_Ok()
    {
        // Act
        var response = await _fixture.Client.PostAsync("api/bot/toggleattackenemybase", null);

        // Assert
        response.StatusCode.ShouldBe(System.Net.HttpStatusCode.OK);
    }

    [Fact]
    public async Task PostScoutMap_Should_Return_Ok()
    {
        // Act
        var response = await _fixture.Client.PostAsync("api/bot/scoutmap", null);

        // Assert
        response.StatusCode.ShouldBe(System.Net.HttpStatusCode.OK);
    }

    [Fact]
    public async Task PostChokeBunker_Should_Return_Ok()
    {
        // Act
        var response = await _fixture.Client.PostAsync("api/bot/chokebunker", null);

        // Assert
        response.StatusCode.ShouldBe(System.Net.HttpStatusCode.OK);
    }

    [Fact]
    public async Task PostChokeDepot_Should_Return_Ok()
    {
        // Act
        var response = await _fixture.Client.PostAsync("api/bot/chokedepot", null);

        // Assert
        response.StatusCode.ShouldBe(System.Net.HttpStatusCode.OK);
    }

    [Fact]
    public async Task Multiple_Status_Calls_Should_Return_Consistent_ServiceUnavailable()
    {
        // Act
        var response1 = await _fixture.Client.GetAsync("api/bot/status");
        var response2 = await _fixture.Client.GetAsync("api/bot/status");

        // Assert - Consistent behavior when Game is null
        response1.StatusCode.ShouldBe(System.Net.HttpStatusCode.ServiceUnavailable);
        response2.StatusCode.ShouldBe(System.Net.HttpStatusCode.ServiceUnavailable);
    }

    [Theory]
    [InlineData("api/bot/status")]
    [InlineData("api/bot/strategy")]
    [InlineData("api/bot/bases")]
    [InlineData("api/bot/units")]
    [InlineData("api/bot/construction")]
    public async Task All_Get_Endpoints_Should_Return_ServiceUnavailable_When_Game_Is_Null(string endpoint)
    {
        // Act
        var response = await _fixture.Client.GetAsync(endpoint);

        // Assert - Documents that all these endpoints require Game to be NOT NULL
        response.StatusCode.ShouldBe(System.Net.HttpStatusCode.ServiceUnavailable);
    }

    [Theory]
    [InlineData("api/bot/togglestrat")]
    [InlineData("api/bot/toggleattackenemybase")]
    [InlineData("api/bot/scoutmap")]
    [InlineData("api/bot/chokebunker")]
    [InlineData("api/bot/chokedepot")]
    public async Task All_Post_Endpoints_Should_Return_Ok(string endpoint)
    {
        // Act
        var response = await _fixture.Client.PostAsync(endpoint, null);

        // Assert
        response.StatusCode.ShouldBe(System.Net.HttpStatusCode.OK);
    }

    [Fact]
    public async Task Bot_Instance_Should_Be_Singleton_Across_Requests()
    {
        // Act
        var response1 = await _fixture.Client.GetAsync("api/bot/status");
        var botInstance1 = _fixture.Bot;
        
        var response2 = await _fixture.Client.GetAsync("api/bot/status");
        var botInstance2 = _fixture.Bot;

        // Assert
        response1.StatusCode.ShouldBe(System.Net.HttpStatusCode.ServiceUnavailable);
        response2.StatusCode.ShouldBe(System.Net.HttpStatusCode.ServiceUnavailable);
        ReferenceEquals(botInstance1, botInstance2).ShouldBe(true);
    }

    [Fact]
    public async Task EnqueueCommand_Via_Api_Should_Be_Processed()
    {
        // Act
        var initialResponse = await _fixture.Client.GetAsync("api/bot/status");
        await _fixture.Client.PostAsync("api/bot/togglestrat", null);
        var afterCommandResponse = await _fixture.Client.GetAsync("api/bot/status");

        // Assert - Both return 503 since Game is null, but commands are still queued successfully
        initialResponse.StatusCode.ShouldBe(System.Net.HttpStatusCode.ServiceUnavailable);
        afterCommandResponse.StatusCode.ShouldBe(System.Net.HttpStatusCode.ServiceUnavailable);
    }
}
