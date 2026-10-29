namespace Northstar.RetailCatalog.Application.Products.DTOs;

public sealed record UpdateProductInputDto(
    string Sku,
    string Name,
    string? Description,
    Guid CategoryId);
