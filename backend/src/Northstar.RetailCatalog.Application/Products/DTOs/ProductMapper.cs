using Northstar.RetailCatalog.Domain.Entities;

namespace Northstar.RetailCatalog.Application.Products.DTOs;

public static class ProductMapper
{
    public static ProductDto ToDto(this Product product, string categoryName) => new(
        product.Id,
        product.Sku,
        product.Name,
        product.Description,
        product.CategoryId,
        categoryName,
        product.Price,
        product.IsActive,
        product.CreatedAtUtc,
        product.UpdatedAtUtc);

    public static ProductPriceHistoryDto ToDto(this ProductPriceHistory entry) => new(
        entry.Id,
        entry.OldPrice,
        entry.NewPrice,
        entry.ChangedAtUtc);
}
