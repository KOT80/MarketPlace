using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Northstar.RetailCatalog.Infrastructure.Data;

namespace Northstar.RetailCatalog.IntegrationTests.Fixtures;

public class RetailCatalogApiFactory : WebApplicationFactory<Program>
{
    private readonly string _connectionString;

    public RetailCatalogApiFactory(string connectionString)
    {
        _connectionString = connectionString;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting("ConnectionStrings:Postgres", _connectionString);
        builder.UseEnvironment("Development");

        builder.ConfigureServices(services =>
        {
            var descriptor = services.Single(d => d.ServiceType == typeof(DbContextOptions<RetailCatalogDbContext>));
            services.Remove(descriptor);

            services.AddDbContext<RetailCatalogDbContext>(options => options.UseNpgsql(_connectionString));

            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<RetailCatalogDbContext>();
            db.Database.Migrate();
        });
    }
}
