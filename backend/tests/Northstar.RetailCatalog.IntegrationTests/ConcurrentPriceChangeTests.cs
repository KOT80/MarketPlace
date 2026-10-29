using System.Net;
using System.Net.Http.Json;
using Northstar.RetailCatalog.Application.Products.Commands.ChangePrice;
using Northstar.RetailCatalog.Application.Products.DTOs;
using Northstar.RetailCatalog.IntegrationTests.Fixtures;
using Xunit;

namespace Northstar.RetailCatalog.IntegrationTests;

public class ConcurrentPriceChangeTests : IClassFixture<TestDatabaseFixture>
{
    private readonly TestDatabaseFixture _fixture;

    public ConcurrentPriceChangeTests(TestDatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task ParallelPriceChanges_ProduceConsistentHistoryChain()
    {
        var client = _fixture.CreateClient();
        var categoryId = await _fixture.SeedCategoryAsync("Конкурентная");
        var created = await client.PostAsJsonAsync("/api/products", new CreateProductInputDto(
            "SKU-CONC", "Товар", null, categoryId, 100m));
        var product = await created.Content.ReadFromJsonAsync<ProductDto>();

        var targets = new[] { 111m, 222m, 333m, 444m, 555m };
        var responses = await Task.WhenAll(targets.Select(price =>
            client.PatchAsJsonAsync($"/api/products/{product!.Id}/price", new ChangePriceRequest(price))));

        var statusCodes = responses.Select(r => r.StatusCode).ToList();

        Assert.All(statusCodes, code =>
            Assert.Contains(code, new[] { HttpStatusCode.OK, HttpStatusCode.Conflict }));

        var rows = await _fixture.ReadPriceHistoryAsync(product!.Id);
        var successes = statusCodes.Count(c => c == HttpStatusCode.OK);

        Assert.Equal(successes, rows.Count);

        if (rows.Count > 0)
        {
            Assert.Equal(100m, rows[0].OldPrice);
            for (var i = 1; i < rows.Count; i++)
            {
                Assert.Equal(rows[i - 1].NewPrice, rows[i].OldPrice);
            }
        }

        var current = await client.GetFromJsonAsync<ProductDto>($"/api/products/{product.Id}");
        if (rows.Count > 0)
        {
            Assert.Equal(rows[^1].NewPrice, current!.Price);
        }
        else
        {
            Assert.Equal(100m, current!.Price);
        }
    }
}
