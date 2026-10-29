using System.Net;
using System.Net.Http.Json;
using Northstar.RetailCatalog.Application.Products.Commands.ChangePrice;
using Northstar.RetailCatalog.Application.Products.DTOs;
using Northstar.RetailCatalog.IntegrationTests.Fixtures;
using Xunit;

namespace Northstar.RetailCatalog.IntegrationTests;

public class PriceHistoryTests : IClassFixture<TestDatabaseFixture>
{
    private readonly TestDatabaseFixture _fixture;

    public PriceHistoryTests(TestDatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    private async Task<(HttpClient Client, ProductDto Product)> CreateProductAsync(decimal price = 100m)
    {
        var client = _fixture.CreateClient();
        var categoryId = await _fixture.SeedCategoryAsync($"Категория {Guid.NewGuid():N}");
        var response = await client.PostAsJsonAsync("/api/products", new CreateProductInputDto(
            $"SKU-{Guid.NewGuid():N}", "Товар", null, categoryId, price));
        var dto = await response.Content.ReadFromJsonAsync<ProductDto>();
        return (client, dto!);
    }

    [Fact]
    public async Task CreateProduct_DoesNotWriteHistory()
    {
        var (client, product) = await CreateProductAsync();

        var history = await client.GetFromJsonAsync<List<ProductPriceHistoryDto>>(
            $"/api/products/{product.Id}/price-history");

        Assert.NotNull(history);
        Assert.Empty(history);
    }

    [Fact]
    public async Task ChangePrice_WritesHistoryAtomically()
    {
        var (client, product) = await CreateProductAsync(100m);

        var response = await client.PatchAsJsonAsync(
            $"/api/products/{product.Id}/price", new ChangePriceRequest(250m));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var updated = await response.Content.ReadFromJsonAsync<ProductDto>();
        Assert.Equal(250m, updated!.Price);

        var rows = await _fixture.ReadPriceHistoryAsync(product.Id);
        var row = Assert.Single(rows);
        Assert.Equal(100m, row.OldPrice);
        Assert.Equal(250m, row.NewPrice);
    }

    [Fact]
    public async Task ChangePrice_SameValue_DoesNotWriteHistory()
    {
        var (client, product) = await CreateProductAsync(100m);
        await client.PatchAsJsonAsync($"/api/products/{product.Id}/price", new ChangePriceRequest(150m));

        var response = await client.PatchAsJsonAsync(
            $"/api/products/{product.Id}/price", new ChangePriceRequest(150m));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var rows = await _fixture.ReadPriceHistoryAsync(product.Id);
        Assert.Single(rows);
    }

    [Fact]
    public async Task ChangePrice_MissingPrice_Returns400()
    {
        var (client, product) = await CreateProductAsync();

        var response = await client.PatchAsJsonAsync(
            $"/api/products/{product.Id}/price", new ChangePriceRequest(null));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task ChangePrice_Negative_Returns400_AndKeepsOldPrice()
    {
        var (client, product) = await CreateProductAsync(100m);

        var response = await client.PatchAsJsonAsync(
            $"/api/products/{product.Id}/price", new ChangePriceRequest(-5m));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var get = await client.GetFromJsonAsync<ProductDto>($"/api/products/{product.Id}");
        Assert.Equal(100m, get!.Price);
        var rows = await _fixture.ReadPriceHistoryAsync(product.Id);
        Assert.Empty(rows);
    }

    [Fact]
    public async Task ChangePrice_ThreeDecimals_Returns400()
    {
        var (client, product) = await CreateProductAsync(100m);

        var response = await client.PatchAsJsonAsync(
            $"/api/products/{product.Id}/price", new ChangePriceRequest(10.005m));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task PriceHistory_ReturnedNewestFirst()
    {
        var (client, product) = await CreateProductAsync(100m);
        await client.PatchAsJsonAsync($"/api/products/{product.Id}/price", new ChangePriceRequest(150m));
        await Task.Delay(50);
        await client.PatchAsJsonAsync($"/api/products/{product.Id}/price", new ChangePriceRequest(200m));

        var history = await client.GetFromJsonAsync<List<ProductPriceHistoryDto>>(
            $"/api/products/{product.Id}/price-history");

        Assert.NotNull(history);
        Assert.Equal(2, history.Count);
        Assert.Equal(200m, history[0].NewPrice);
        Assert.Equal(150m, history[1].NewPrice);
    }

    [Fact]
    public async Task PriceHistory_UnknownProduct_Returns404()
    {
        var client = _fixture.CreateClient();

        var response = await client.GetAsync($"/api/products/{Guid.NewGuid()}/price-history");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
