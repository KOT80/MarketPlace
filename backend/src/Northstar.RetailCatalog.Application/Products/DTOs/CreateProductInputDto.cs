namespace Northstar.RetailCatalog.Application.Products.DTOs;

public sealed record CreateProductInputDto(
    string Sku,
    string Name,
    string? Description,
    Guid CategoryId,
    decimal Price);
