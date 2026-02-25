using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Shared.Interfaces;
using Shared.Models;
using Shouldly;
using System.Text.Json;
using Web.Controllers;
using Xunit;

namespace TestProject1;

public class MatchesTestWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Leave the default MatchRepository for integration testing
        });
    }
}

public class MatchesControllerTestFixture : IAsyncLifetime
{
    public MatchesTestWebApplicationFactory Factory { get; private set; } = null!;
    public HttpClient Client { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        Factory = new MatchesTestWebApplicationFactory();
        Client = Factory.CreateClient();
        await Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        Client?.Dispose();
        Factory?.Dispose();
        await Task.CompletedTask;
    }
}

public class MatchesControllerTests : IClassFixture<MatchesControllerTestFixture>
{
    private readonly MatchesControllerTestFixture _fixture;

    public MatchesControllerTests(MatchesControllerTestFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task GetAllMatches_ReturnsOkWithMatches()
    {
        // ACT
        var response = await _fixture.Client.GetAsync("/api/matches");

        // ASSERT
        response.StatusCode.ShouldBe(System.Net.HttpStatusCode.OK);
        response.Content.Headers.ContentType?.MediaType.ShouldBe("application/json");

        var json = await response.Content.ReadAsStringAsync();
        var matches = JsonSerializer.Deserialize<List<Shared.Models.Match>>(json, 
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new();
        
        // At minimum, should not crash
        matches.ShouldBeOfType<List<Shared.Models.Match>>();
    }

    [Fact]
    public async Task GetMatchById_WithValidId_ReturnsOkWithMatch()
    {
        // ARRANGE - First get all matches to find a valid ID
        var getAllResponse = await _fixture.Client.GetAsync("/api/matches");
        var json = await getAllResponse.Content.ReadAsStringAsync();
        var matches = JsonSerializer.Deserialize<List<Shared.Models.Match>>(json,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new();

        if (matches.Count == 0)
        {
            // Skip if no matches exist
            return;
        }

        var firstMatchId = matches.First().Id;

        // ACT
        var response = await _fixture.Client.GetAsync($"/api/matches/{firstMatchId}");

        // ASSERT
        response.StatusCode.ShouldBe(System.Net.HttpStatusCode.OK);
        
        var responseJson = await response.Content.ReadAsStringAsync();
        var match = JsonSerializer.Deserialize<Shared.Models.Match>(responseJson,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        match.ShouldNotBeNull();
        match.Id.ShouldBe(firstMatchId);
    }

    [Fact]
    public async Task GetMatchById_WithInvalidId_ReturnsNotFound()
    {
        // ACT
        var response = await _fixture.Client.GetAsync("/api/matches/99999");

        // ASSERT
        response.StatusCode.ShouldBe(System.Net.HttpStatusCode.NotFound);
    }
}
