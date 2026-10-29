using System.Net.Http.Json;
using Northstar.RetailCatalog.Application.Products.DTOs;
using Northstar.RetailCatalog.IntegrationTests.Fixtures;
using Xunit;

namespace Northstar.RetailCatalog.IntegrationTests;

public class SearchEscapeTests : IClassFixture<TestDatabaseFixture>
{
    private readonly TestDatabaseFixture _fixture;

    public SearchEscapeTests(TestDatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Search_PercentIsLiteral_NotWildcard()
    {
        var client = _fixture.CreateClient();
        var categoryId = await _fixture.SeedCategoryAsync("Акции");

        await client.PostAsJsonAsync("/api/products", new CreateProductInputDto(
            "PCT-LITERAL", "Промо 25% скидка", null, categoryId, 10m));
        await client.PostAsJsonAsync("/api/products", new CreateProductInputDto(
            "PCT-WILD", "Промо 25 процентов", null, categoryId, 20m));

        var result = await client.GetFromJsonAsync<Paged<ProductListItemDto>>(
            "/api/products?search=25%25&page=1&pageSize=10");

        Assert.NotNull(result);
        Assert.Equal(1, result!.TotalCount);
        Assert.Equal("PCT-LITERAL", result.Items[0].Sku);
    }

    [Fact]
    public async Task Search_UnderscoreIsLiteral_NotSingleCharWildcard()
    {
        var client = _fixture.CreateClient();
        var categoryId = await _fixture.SeedCategoryAsync("Шаблон");

        await client.PostAsJsonAsync("/api/products", new CreateProductInputDto(
            "US-LITERAL", "Тест_value", null, categoryId, 10m));
        await client.PostAsJsonAsync("/api/products", new CreateProductInputDto(
            "US-WILD", "Тест value", null, categoryId, 20m));

        var result = await client.GetFromJsonAsync<Paged<ProductListItemDto>>(
            "/api/products?search=%D0%A2%D0%B5%D1%81%D1%82_value&page=1&pageSize=10");

        Assert.Equal(1, result!.TotalCount);
        Assert.Equal("US-LITERAL", result.Items[0].Sku);
    }

    private sealed record Paged<T>(List<T> Items, int TotalCount);
}
