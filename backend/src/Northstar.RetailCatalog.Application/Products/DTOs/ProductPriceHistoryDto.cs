namespace Northstar.RetailCatalog.Application.Products.DTOs;

public sealed record ProductPriceHistoryDto(
    Guid Id,
    decimal OldPrice,
    decimal NewPrice,
    DateTime ChangedAtUtc);
