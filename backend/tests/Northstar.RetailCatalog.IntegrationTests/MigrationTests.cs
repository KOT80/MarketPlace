using System.Globalization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Northstar.RetailCatalog.IntegrationTests.Fixtures;
using Northstar.RetailCatalog.Infrastructure.Data;
using Xunit;

namespace Northstar.RetailCatalog.IntegrationTests;

public class MigrationTests : IClassFixture<TestDatabaseFixture>
{
    private readonly TestDatabaseFixture _fixture;

    public MigrationTests(TestDatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Migrate_CreatesExpectedTables()
    {
        using var scope = _fixture.Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<RetailCatalogDbContext>();

        var connection = db.Database.GetDbConnection();
        await connection.OpenAsync();

        await using (var command = connection.CreateCommand())
        {
            command.CommandText =
                "SELECT COUNT(*) FROM information_schema.tables " +
                "WHERE table_schema = 'public' AND table_name IN ('products', 'categories', 'product_price_history')";
            var count = Convert.ToInt32(await command.ExecuteScalarAsync(), CultureInfo.InvariantCulture);
            Assert.Equal(3, count);
        }

        await using (var command = connection.CreateCommand())
        {
            command.CommandText =
                "SELECT COUNT(*) FROM pg_indexes " +
                "WHERE schemaname = 'public' AND indexname = 'ux_products_sku'";
            var count = Convert.ToInt32(await command.ExecuteScalarAsync(), CultureInfo.InvariantCulture);
            Assert.Equal(1, count);
        }
    }
}
