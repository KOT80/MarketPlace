namespace Northstar.RetailCatalog.Application.Products.DTOs;

public sealed record ProductDto(
    Guid Id,
    string Sku,
    string Name,
    string? Description,
    Guid CategoryId,
    string CategoryName,
    decimal Price,
    bool IsActive,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc);
