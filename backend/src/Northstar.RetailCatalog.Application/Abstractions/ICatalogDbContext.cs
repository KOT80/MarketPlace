using Microsoft.EntityFrameworkCore;
using Northstar.RetailCatalog.Domain.Entities;

namespace Northstar.RetailCatalog.Application.Abstractions;

public interface ICatalogDbContext
{
    DbSet<Product> Products { get; }

    DbSet<Category> Categories { get; }

    DbSet<ProductPriceHistory> ProductPriceHistory { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    Task ReloadAsync(Product product, CancellationToken cancellationToken = default);
}
