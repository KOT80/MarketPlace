using System.Net;
using System.Net.Http.Json;
using Northstar.RetailCatalog.Application.Categories.DTOs;
using Northstar.RetailCatalog.Application.Products.DTOs;
using Northstar.RetailCatalog.IntegrationTests.Fixtures;
using Xunit;

namespace Northstar.RetailCatalog.IntegrationTests;

public class CatalogQueryTests : IClassFixture<CatalogDataFixture>
{
    private readonly TestDatabaseFixture _fixture;
    private readonly CatalogDataFixture _data;

    public CatalogQueryTests(CatalogDataFixture data)
    {
        _data = data;
        _fixture = data.Db;
    }

    private HttpClient Client => _fixture.CreateClient();

    [Fact]
    public async Task Pagination_ReturnsCorrectWindowAndTotals()
    {
        var page = await Client.GetFromJsonAsync<Paged<ProductListItemDto>>(
            "/api/products?page=1&pageSize=10");

        Assert.NotNull(page);
        Assert.Equal(10, page.Items.Count);
        Assert.Equal(25, page.TotalCount);
        Assert.Equal(3, page.TotalPages);
        Assert.True(page.HasNext);
        Assert.False(page.HasPrevious);

        var last = await Client.GetFromJsonAsync<Paged<ProductListItemDto>>(
            "/api/products?page=3&pageSize=10");
        Assert.Equal(5, last!.Items.Count);
    }

    [Fact]
    public async Task Search_IsCaseInsensitive_OnNameAndSku()
    {
        var byName = await Client.GetFromJsonAsync<Paged<ProductListItemDto>>(
            "/api/products?search=widget&page=1&pageSize=50");
        Assert.Equal(5, byName!.TotalCount);

        var bySku = await Client.GetFromJsonAsync<Paged<ProductListItemDto>>(
            "/api/products?search=sku-00&page=1&pageSize=50");
        Assert.Equal(10, bySku!.TotalCount);
    }

    [Fact]
    public async Task Search_UsesLiteralPercent()
    {
        var none = await Client.GetFromJsonAsync<Paged<ProductListItemDto>>(
            "/api/products?search=%25&page=1&pageSize=50");
        Assert.Equal(0, none!.TotalCount);
    }

    [Fact]
    public async Task FilterByCategory_Works()
    {
        var result = await Client.GetFromJsonAsync<Paged<ProductListItemDto>>(
            $"/api/products?categoryId={_data.CategoryA}&page=1&pageSize=50");

        Assert.Equal(13, result!.TotalCount);
        Assert.All(result.Items, i => Assert.Equal(_data.CategoryA, i.CategoryId));
    }

    [Fact]
    public async Task FilterByIsActive_Works()
    {
        var inactive = await Client.GetFromJsonAsync<Paged<ProductListItemDto>>(
            "/api/products?isActive=false&page=1&pageSize=50");

        Assert.Equal(1, inactive!.TotalCount);
        Assert.Equal("SKU-007", inactive.Items[0].Sku);
    }

    [Fact]
    public async Task SortByPriceAscending_IsMonotonic()
    {
        var all = await Client.GetFromJsonAsync<Paged<ProductListItemDto>>(
            "/api/products?sortBy=price&sortDirection=asc&page=1&pageSize=100");

        Assert.NotNull(all);
        var prices = all!.Items.Select(p => p.Price).ToList();
        Assert.Equal(prices.OrderBy(p => p).ToList(), prices);
    }

    [Fact]
    public async Task SortingWithTies_IsStableAcrossPageBoundaries()
    {
        var paged = new List<ProductListItemDto>();
        for (var page = 1; page <= 3; page++)
        {
            var chunk = await Client.GetFromJsonAsync<Paged<ProductListItemDto>>(
                $"/api/products?sortBy=price&sortDirection=asc&page={page}&pageSize=9");
            paged.AddRange(chunk!.Items);
        }

        var whole = await Client.GetFromJsonAsync<Paged<ProductListItemDto>>(
            "/api/products?sortBy=price&sortDirection=asc&page=1&pageSize=100");

        Assert.Equal(whole!.TotalCount, paged.Count);
        Assert.Equal(whole.Items.Select(p => p.Id), paged.Select(p => p.Id));
    }

    [Fact]
    public async Task SortByCreatedAtDescending_DefaultsApplied()
    {
        var page = await Client.GetFromJsonAsync<Paged<ProductListItemDto>>(
            "/api/products?page=1&pageSize=25");

        Assert.NotNull(page);
        var created = page!.Items.Select(p => p.CreatedAtUtc).ToList();
        Assert.Equal(created.OrderByDescending(t => t).ToList(), created);
    }

    [Fact]
    public async Task InvalidSortBy_Returns400()
    {
        var response = await Client.GetAsync("/api/products?sortBy=price%3B%20DROP");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Theory]
    [InlineData("page=0")]
    [InlineData("pageSize=0")]
    [InlineData("pageSize=101")]
    [InlineData("sortDirection=sideways")]
    public async Task InvalidPagingParameters_Return400(string query)
    {
        var response = await Client.GetAsync($"/api/products?{query}");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Categories_ReturnedSortedByName()
    {
        var categories = await Client.GetFromJsonAsync<List<CategoryDto>>("/api/categories");

        Assert.NotNull(categories);
        Assert.Contains(categories, c => c.Name == "Категория А");
        Assert.Contains(categories, c => c.Name == "Категория Б");
        var names = categories!.Select(c => c.Name).ToList();
        Assert.Equal(names.OrderBy(n => n, StringComparer.Ordinal).ToList(), names);
    }

    private sealed record Paged<T>(
        List<T> Items,
        int Page,
        int PageSize,
        int TotalCount,
        int TotalPages,
        bool HasPrevious,
        bool HasNext);
}
