using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Northstar.RetailCatalog.Application.Abstractions;
using Northstar.RetailCatalog.Infrastructure.Data;

namespace Northstar.RetailCatalog.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Postgres")
            ?? throw new InvalidOperationException("Connection string 'Postgres' is not configured.");

        services.AddDbContext<RetailCatalogDbContext>(options => options.UseNpgsql(connectionString));

        services.AddScoped<ICatalogDbContext>(sp => sp.GetRequiredService<RetailCatalogDbContext>());
        services.AddScoped<IProductCatalogQuery, ProductCatalogQuery>();

        return services;
    }
}
