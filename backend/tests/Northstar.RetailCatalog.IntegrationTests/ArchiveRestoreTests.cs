using System.Net;
using System.Net.Http.Json;
using Northstar.RetailCatalog.Application.Products.Commands.ChangePrice;
using Northstar.RetailCatalog.Application.Products.DTOs;
using Northstar.RetailCatalog.IntegrationTests.Fixtures;
using Xunit;

namespace Northstar.RetailCatalog.IntegrationTests;

public class ArchiveRestoreTests : IClassFixture<TestDatabaseFixture>
{
    private readonly TestDatabaseFixture _fixture;

    public ArchiveRestoreTests(TestDatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    private async Task<(HttpClient Client, ProductDto Product)> CreateProductAsync()
    {
        var client = _fixture.CreateClient();
        var categoryId = await _fixture.SeedCategoryAsync($"Категория {Guid.NewGuid():N}");
        var response = await client.PostAsJsonAsync("/api/products", new CreateProductInputDto(
            $"SKU-{Guid.NewGuid():N}", "Товар", null, categoryId, 100m));
        var dto = await response.Content.ReadFromJsonAsync<ProductDto>();
        return (client, dto!);
    }

    [Fact]
    public async Task Archive_MarksInactive()
    {
        var (client, product) = await CreateProductAsync();

        var response = await client.PostAsync($"/api/products/{product.Id}/archive", null);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var dto = await response.Content.ReadFromJsonAsync<ProductDto>();
        Assert.False(dto!.IsActive);
    }

    [Fact]
    public async Task Archive_Twice_Returns409()
    {
        var (client, product) = await CreateProductAsync();
        await client.PostAsync($"/api/products/{product.Id}/archive", null);

        var response = await client.PostAsync($"/api/products/{product.Id}/archive", null);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Restore_UnknownProduct_Returns404()
    {
        var client = _fixture.CreateClient();

        var response = await client.PostAsync($"/api/products/{Guid.NewGuid()}/restore", null);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Restore_ActiveProduct_Returns409()
    {
        var (client, product) = await CreateProductAsync();

        var response = await client.PostAsync($"/api/products/{product.Id}/restore", null);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task ArchiveRestore_PreservesPriceHistory()
    {
        var (client, product) = await CreateProductAsync();
        await client.PatchAsJsonAsync($"/api/products/{product.Id}/price", new ChangePriceRequest(150m));

        await client.PostAsync($"/api/products/{product.Id}/archive", null);
        await client.PostAsync($"/api/products/{product.Id}/restore", null);

        var rows = await _fixture.ReadPriceHistoryAsync(product.Id);
        Assert.Single(rows);
        Assert.Equal(150m, rows[0].NewPrice);
    }

    [Fact]
    public async Task ArchivedProduct_FiltersCorrectly()
    {
        var (client, product) = await CreateProductAsync();
        await client.PostAsync($"/api/products/{product.Id}/archive", null);

        var active = await client.GetFromJsonAsync<CatalogPage>($"/api/products?isActive=true&page=1&pageSize=100");
        var inactive = await client.GetFromJsonAsync<CatalogPage>($"/api/products?isActive=false&page=1&pageSize=100");

        Assert.DoesNotContain(active!.Items, i => i.Id == product.Id);
        Assert.Contains(inactive!.Items, i => i.Id == product.Id);
    }

    private sealed record CatalogPage(List<ProductListItemDto> Items, int Page, int PageSize, int TotalCount);
}
