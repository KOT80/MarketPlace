using System.Net;
using System.Net.Http.Json;
using Northstar.RetailCatalog.IntegrationTests.Fixtures;
using Xunit;

namespace Northstar.RetailCatalog.IntegrationTests;

public class HealthEndpointTests : IClassFixture<TestDatabaseFixture>
{
    private readonly TestDatabaseFixture _fixture;

    public HealthEndpointTests(TestDatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Health_ReturnsHealthy_WhenDatabaseAvailable()
    {
        using var client = _fixture.CreateClient();

        var response = await client.GetAsync("/api/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<HealthResponse>();
        Assert.NotNull(body);
        Assert.Equal("Healthy", body.Status);
        Assert.Equal("Available", body.Database);
    }

    [Fact]
    public async Task Health_ReturnsExpectedJsonShape()
    {
        using var client = _fixture.CreateClient();

        var response = await client.GetAsync("/api/health");

        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("\"status\"", content);
        Assert.Contains("\"database\"", content);
        Assert.Contains("\"timestampUtc\"", content);
    }

    private sealed record HealthResponse(string Status, string Database, DateTime TimestampUtc);
}
