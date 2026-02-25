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
    public async Task SetStrategy_Should_Return_Ok_With_Valid_Strategy()
    {
        // Arrange
        var strategyRequest = new SetStrategyRequest { Strategy = Strategy.Aggressive };
        var content = new StringContent(JsonSerializer.Serialize(strategyRequest), System.Text.Encoding.UTF8, "application/json");

        // Act
        var response = await _fixture.Client.PutAsync("api/bot/strategy", content);

        // Assert
        response.StatusCode.ShouldBe(System.Net.HttpStatusCode.OK);
        var responseContent = await response.Content.ReadAsStringAsync();
        responseContent.ShouldContain("change strategy");
    }

    [Fact]
    public async Task SetStrategy_Should_Return_BadRequest_When_Strategy_Is_Null()
    {
        // Arrange
        var strategyRequest = new SetStrategyRequest { Strategy = null };
        var content = new StringContent(JsonSerializer.Serialize(strategyRequest), System.Text.Encoding.UTF8, "application/json");

        // Act
        var response = await _fixture.Client.PutAsync("api/bot/strategy", content);

        // Assert
        response.StatusCode.ShouldBe(System.Net.HttpStatusCode.BadRequest);
        var responseContent = await response.Content.ReadAsStringAsync();
        responseContent.ShouldContain("cannot be null");
    }
}
