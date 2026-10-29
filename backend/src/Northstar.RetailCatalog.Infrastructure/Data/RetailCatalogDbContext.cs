using Microsoft.EntityFrameworkCore;
using Northstar.RetailCatalog.Application.Abstractions;
using Northstar.RetailCatalog.Domain.Entities;

namespace Northstar.RetailCatalog.Infrastructure.Data;

public class RetailCatalogDbContext(DbContextOptions<RetailCatalogDbContext> options)
    : DbContext(options), ICatalogDbContext
{
    public DbSet<Product> Products => Set<Product>();

    public DbSet<Category> Categories => Set<Category>();

    public DbSet<ProductPriceHistory> ProductPriceHistory => Set<ProductPriceHistory>();

    public Task ReloadAsync(Product product, CancellationToken cancellationToken = default) =>
        Entry(product).ReloadAsync(cancellationToken);

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.HasPostgresExtension("pg_trgm");

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(RetailCatalogDbContext).Assembly);
    }
}
