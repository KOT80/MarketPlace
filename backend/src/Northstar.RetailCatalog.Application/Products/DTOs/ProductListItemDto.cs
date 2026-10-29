namespace Northstar.RetailCatalog.Application.Products.DTOs;

public sealed record ProductListItemDto(
    Guid Id,
    string Sku,
    string Name,
    Guid CategoryId,
    string CategoryName,
    decimal Price,
    bool IsActive,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc);
