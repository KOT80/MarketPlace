using Northstar.RetailCatalog.IntegrationTests.Fixtures;
using Xunit;

namespace Northstar.RetailCatalog.IntegrationTests;

public sealed class CatalogDataFixture : IAsyncLifetime
{
    public TestDatabaseFixture Db { get; private set; } = null!;

    public Guid CategoryA { get; private set; }

    public Guid CategoryB { get; private set; }

    public async Task InitializeAsync()
    {
        Db = new TestDatabaseFixture();
        await Db.InitializeAsync();

        CategoryA = await Db.SeedCategoryAsync("Категория А");
        CategoryB = await Db.SeedCategoryAsync("Категория Б");

        for (var i = 0; i < 25; i++)
        {
            await Db.SeedProductAsync(
                i % 2 == 0 ? CategoryA : CategoryB,
                $"SKU-{i:D3}",
                i % 5 == 0 ? $"Widget {i}" : $"Gadget {i}",
                price: 100m + (i % 5) * 50m,
                isActive: i != 7,
                createdAtUtc: new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddHours(i));
        }
    }

    public async Task DisposeAsync() => await Db.DisposeAsync();
}
