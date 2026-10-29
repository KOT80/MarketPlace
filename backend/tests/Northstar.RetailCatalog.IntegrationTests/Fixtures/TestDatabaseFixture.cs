using Npgsql;

namespace Northstar.RetailCatalog.IntegrationTests.Fixtures;

public sealed class TestDatabaseFixture : IAsyncLifetime
{
    private TestDatabase? _database;

    public RetailCatalogApiFactory Factory { get; private set; } = null!;

    public string ConnectionString => _database!.ConnectionString;

    public HttpClient CreateClient() => Factory.CreateClient();

    public async Task InitializeAsync()
    {
        _database = await TestDatabase.CreateAsync();
        Factory = new RetailCatalogApiFactory(_database.ConnectionString);
        using var client = CreateClient();
        using var warmup = await client.GetAsync("/api/health");
    }

    public async Task DisposeAsync()
    {
        await Factory.DisposeAsync();
        if (_database is not null)
        {
            await _database.DisposeAsync();
        }
    }

    public async Task ExecuteAsync(string sql, params NpgsqlParameter[] parameters)
    {
        await using var connection = new NpgsqlConnection(ConnectionString);
        await connection.OpenAsync();
        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddRange(parameters);
        await command.ExecuteNonQueryAsync();
    }

    public async Task<Guid> SeedCategoryAsync(string name, DateTime? createdAtUtc = null)
    {
        var id = Guid.NewGuid();
        await ExecuteAsync(
            "INSERT INTO categories (id, name, created_at_utc) VALUES (@id, @name, @createdAt)",
            new NpgsqlParameter("id", id),
            new NpgsqlParameter("name", name),
            new NpgsqlParameter("createdAt", createdAtUtc ?? new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)));
        return id;
    }

    public async Task<Guid> SeedProductAsync(
        Guid categoryId,
        string sku,
        string name,
        decimal price,
        bool isActive = true,
        DateTime? createdAtUtc = null)
    {
        var id = Guid.NewGuid();
        await ExecuteAsync(
            "INSERT INTO products (id, sku, name, category_id, price, is_active, created_at_utc) " +
            "VALUES (@id, @sku, @name, @categoryId, @price, @isActive, @createdAt)",
            new NpgsqlParameter("id", id),
            new NpgsqlParameter("sku", sku.ToUpperInvariant()),
            new NpgsqlParameter("name", name),
            new NpgsqlParameter("categoryId", categoryId),
            new NpgsqlParameter("price", price),
            new NpgsqlParameter("isActive", isActive),
            new NpgsqlParameter("createdAt", createdAtUtc ?? new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)));
        return id;
    }

    public async Task<List<HistoryRow>> ReadPriceHistoryAsync(Guid productId)
    {
        await using var connection = new NpgsqlConnection(ConnectionString);
        await connection.OpenAsync();
        await using var command = new NpgsqlCommand(
            "SELECT old_price, new_price, changed_at_utc FROM product_price_history " +
            "WHERE product_id = @id ORDER BY changed_at_utc ASC, id ASC", connection);
        command.Parameters.AddWithValue("id", productId);

        var rows = new List<HistoryRow>();
        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            rows.Add(new HistoryRow(
                reader.GetDecimal(0),
                reader.GetDecimal(1),
                DateTime.SpecifyKind(reader.GetDateTime(2), DateTimeKind.Utc)));
        }

        return rows;
    }

    public sealed record HistoryRow(decimal OldPrice, decimal NewPrice, DateTime ChangedAtUtc);
}
