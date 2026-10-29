using System.Net;
using System.Net.Http.Json;
using Northstar.RetailCatalog.Application.Products.DTOs;
using Northstar.RetailCatalog.IntegrationTests.Fixtures;
using Xunit;

namespace Northstar.RetailCatalog.IntegrationTests;

public class ProductLifecycleTests : IClassFixture<TestDatabaseFixture>
{
    private readonly TestDatabaseFixture _fixture;

    public ProductLifecycleTests(TestDatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task CreateProduct_Returns201_WithLocationHeader()
    {
        var client = _fixture.CreateClient();
        var categoryId = await _fixture.SeedCategoryAsync("Электроника");

        var response = await client.PostAsJsonAsync("/api/products", new CreateProductInputDto(
            "SKU-NEW-1", "Ноутбук", "16 ГБ RAM", categoryId, 74999.99m));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var dto = await response.Content.ReadFromJsonAsync<ProductDto>();
        Assert.NotNull(dto);
        Assert.Equal("SKU-NEW-1", dto.Sku);
        Assert.Equal(74999.99m, dto.Price);
        Assert.True(dto.IsActive);
        Assert.Equal("Электроника", dto.CategoryName);
        Assert.NotNull(response.Headers.Location);
        Assert.EndsWith($"/api/products/{dto.Id}", response.Headers.Location!.ToString());
    }

    [Fact]
    public async Task CreateProduct_NormalizesSku()
    {
        var client = _fixture.CreateClient();
        var categoryId = await _fixture.SeedCategoryAsync("Книги");

        var response = await client.PostAsJsonAsync("/api/products", new CreateProductInputDto(
            "  book-2026  ", "Книга", null, categoryId, 999.50m));

        var dto = await response.Content.ReadFromJsonAsync<ProductDto>();
        Assert.Equal("BOOK-2026", dto!.Sku);
    }

    [Fact]
    public async Task CreateProduct_CaseInsensitiveSkuConflict_Returns409()
    {
        var client = _fixture.CreateClient();
        var categoryId = await _fixture.SeedCategoryAsync("Мебель");
        await client.PostAsJsonAsync("/api/products", new CreateProductInputDto(
            "sku-dup", "Стул", null, categoryId, 1000m));

        var response = await client.PostAsJsonAsync("/api/products", new CreateProductInputDto(
            " SKU-DUP ", "Диван", null, categoryId, 5000m));

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task CreateProduct_UnknownCategory_Returns400()
    {
        var client = _fixture.CreateClient();

        var response = await client.PostAsJsonAsync("/api/products", new CreateProductInputDto(
            "SKU-ORPHAN", "Товар", null, Guid.NewGuid(), 100m));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetailsStub>();
        Assert.NotNull(problem);
        Assert.Equal(400, problem.Status);
    }

    [Fact]
    public async Task CreateProduct_NegativePrice_Returns400ValidationProblem()
    {
        var client = _fixture.CreateClient();
        var categoryId = await _fixture.SeedCategoryAsync("Продукты");

        var response = await client.PostAsJsonAsync("/api/products", new CreateProductInputDto(
            "SKU-NEG", "Товар", null, categoryId, -1m));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("errors", body);
    }

    [Fact]
    public async Task GetProduct_UnknownId_Returns404()
    {
        var client = _fixture.CreateClient();

        var response = await client.GetAsync($"/api/products/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetProduct_ReturnsDetailsWithCategoryName()
    {
        var client = _fixture.CreateClient();
        var categoryId = await _fixture.SeedCategoryAsync("Инструменты");
        var created = await client.PostAsJsonAsync("/api/products", new CreateProductInputDto(
            "SKU-DETAIL", "Дрель", "18 В", categoryId, 4500m));
        var createdDto = await created.Content.ReadFromJsonAsync<ProductDto>();

        var response = await client.GetAsync($"/api/products/{createdDto!.Id}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var dto = await response.Content.ReadFromJsonAsync<ProductDto>();
        Assert.Equal("Дрель", dto!.Name);
        Assert.Equal("18 В", dto.Description);
        Assert.Equal("Инструменты", dto.CategoryName);
        Assert.Null(dto.UpdatedAtUtc);
    }

    [Fact]
    public async Task UpdateProduct_ChangesAllowedFields_KeepsPriceAndState()
    {
        var client = _fixture.CreateClient();
        var categoryId = await _fixture.SeedCategoryAsync("Кухня");
        var otherCategory = await _fixture.SeedCategoryAsync("Ванная");
        var created = await client.PostAsJsonAsync("/api/products", new CreateProductInputDto(
            "SKU-UPD", "Чайник", null, categoryId, 2000m));
        var createdDto = await created.Content.ReadFromJsonAsync<ProductDto>();

        var response = await client.PutAsJsonAsync($"/api/products/{createdDto!.Id}",
            new UpdateProductInputDto("sku-upd-2", "Электрочайник", "1.7 л", otherCategory));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var dto = await response.Content.ReadFromJsonAsync<ProductDto>();
        Assert.Equal("SKU-UPD-2", dto!.Sku);
        Assert.Equal("Электрочайник", dto.Name);
        Assert.Equal("1.7 л", dto.Description);
        Assert.Equal(otherCategory, dto.CategoryId);
        Assert.Equal("Ванная", dto.CategoryName);
        Assert.Equal(2000m, dto.Price);
        Assert.True(dto.IsActive);
        Assert.NotNull(dto.UpdatedAtUtc);
    }

    [Fact]
    public async Task UpdateProduct_UnknownId_Returns404()
    {
        var client = _fixture.CreateClient();
        var categoryId = await _fixture.SeedCategoryAsync("Сад");

        var response = await client.PutAsJsonAsync($"/api/products/{Guid.NewGuid()}",
            new UpdateProductInputDto("SKU-GHOST", "Призрак", null, categoryId));

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task UpdateProduct_SkuOfAnotherProduct_Returns409()
    {
        var client = _fixture.CreateClient();
        var categoryId = await _fixture.SeedCategoryAsync("Гараж");
        var first = await client.PostAsJsonAsync("/api/products", new CreateProductInputDto(
            "SKU-A", "Первый", null, categoryId, 10m));
        var firstDto = await first.Content.ReadFromJsonAsync<ProductDto>();
        var second = await client.PostAsJsonAsync("/api/products", new CreateProductInputDto(
            "SKU-B", "Второй", null, categoryId, 20m));
        var secondDto = await second.Content.ReadFromJsonAsync<ProductDto>();

        var response = await client.PutAsJsonAsync($"/api/products/{secondDto!.Id}",
            new UpdateProductInputDto("SKU-A", "Второй", null, categoryId));

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    private sealed record ProblemDetailsStub(int? Status, string? Title, string? Detail);
}
