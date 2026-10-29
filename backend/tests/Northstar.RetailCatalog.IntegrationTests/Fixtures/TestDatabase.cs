using Npgsql;

namespace Northstar.RetailCatalog.IntegrationTests.Fixtures;

public sealed class TestDatabase : IAsyncDisposable
{
    private TestDatabase(string name, string connectionString)
    {
        Name = name;
        ConnectionString = connectionString;
    }

    public string Name { get; }

    public string ConnectionString { get; }

    public static async Task<TestDatabase> CreateAsync()
    {
        var baseConnectionString =
            Environment.GetEnvironmentVariable("RETAILCATALOG_TEST_DB")
            ?? "Host=127.0.0.1;Port=5432;Database=postgres;Username=postgres;Password=postgres";

        var name = $"retailcatalog_test_{Guid.NewGuid():N}";

        var b = new NpgsqlConnectionStringBuilder(baseConnectionString) { Database = "postgres" };
        await using var admin = new NpgsqlConnection(b.ConnectionString);
        await admin.OpenAsync();

        await using (var create = new NpgsqlCommand($"CREATE DATABASE \"{name}\"", admin))
        {
            await create.ExecuteNonQueryAsync();
        }

        b.Database = name;
        return new TestDatabase(name, b.ConnectionString);
    }

    public async ValueTask DisposeAsync()
    {
        var b = new NpgsqlConnectionStringBuilder(ConnectionString) { Database = "postgres" };
        await using var admin = new NpgsqlConnection(b.ConnectionString);
        await admin.OpenAsync();

        await using (var terminate = new NpgsqlCommand(
            "SELECT pg_terminate_backend(pid) FROM pg_stat_activity WHERE datname = @name AND pid <> pg_backend_pid()", admin))
        {
            terminate.Parameters.AddWithValue("name", Name);
            await terminate.ExecuteNonQueryAsync();
        }

        await using (var drop = new NpgsqlCommand($"DROP DATABASE IF EXISTS \"{Name}\"", admin))
        {
            await drop.ExecuteNonQueryAsync();
        }
    }
}
