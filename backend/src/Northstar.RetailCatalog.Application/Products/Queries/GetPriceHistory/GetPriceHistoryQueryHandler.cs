using MediatR;
using Microsoft.EntityFrameworkCore;
using Northstar.RetailCatalog.Application.Abstractions;
using Northstar.RetailCatalog.Application.Exceptions;
using Northstar.RetailCatalog.Application.Products.DTOs;
using Northstar.RetailCatalog.Domain.Entities;

namespace Northstar.RetailCatalog.Application.Products.Queries.GetPriceHistory;

public sealed class GetPriceHistoryQueryHandler(
    ICatalogDbContext db) : IRequestHandler<GetPriceHistoryQuery, IReadOnlyList<ProductPriceHistoryDto>>
{
    public async Task<IReadOnlyList<ProductPriceHistoryDto>> Handle(
        GetPriceHistoryQuery request, CancellationToken cancellationToken)
    {
        var productExists = await db.Products.AnyAsync(p => p.Id == request.Id, cancellationToken);
        if (!productExists)
        {
            throw new NotFoundException(nameof(Product), request.Id);
        }

        var entries = await db.ProductPriceHistory.AsNoTracking()
            .Where(h => h.ProductId == request.Id)
            .OrderByDescending(h => h.ChangedAtUtc)
            .ThenByDescending(h => h.Id)
            .Select(h => new ProductPriceHistoryDto(h.Id, h.OldPrice, h.NewPrice, h.ChangedAtUtc))
            .ToListAsync(cancellationToken);

        return entries;
    }
}
