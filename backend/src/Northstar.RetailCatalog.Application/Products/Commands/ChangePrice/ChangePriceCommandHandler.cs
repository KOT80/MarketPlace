using MediatR;
using Microsoft.EntityFrameworkCore;
using Northstar.RetailCatalog.Application.Abstractions;
using Northstar.RetailCatalog.Application.Exceptions;
using Northstar.RetailCatalog.Application.Products.DTOs;
using Northstar.RetailCatalog.Domain.Entities;
using Northstar.RetailCatalog.Domain.Exceptions;

namespace Northstar.RetailCatalog.Application.Products.Commands.ChangePrice;

public sealed class ChangePriceCommandHandler(
    ICatalogDbContext db,
    TimeProvider timeProvider) : IRequestHandler<ChangePriceCommand, ProductDto>
{
    public async Task<ProductDto> Handle(ChangePriceCommand request, CancellationToken cancellationToken)
    {
        var newPrice = request.NewPrice
            ?? throw new DomainRuleException("Цена не может быть отсутствующей.");

        var product = await db.Products.SingleOrDefaultAsync(p => p.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Product), request.Id);

        var history = product.ChangePrice(newPrice, timeProvider.GetUtcNow().UtcDateTime);
        if (history is not null)
        {
            db.ProductPriceHistory.Add(history);
        }

        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            if (history is not null)
            {
                db.ProductPriceHistory.Remove(history);
            }

            await db.ReloadAsync(product, cancellationToken);

            history = product.ChangePrice(newPrice, timeProvider.GetUtcNow().UtcDateTime);
            if (history is not null)
            {
                db.ProductPriceHistory.Add(history);
            }

            await db.SaveChangesAsync(cancellationToken);
        }

        var categoryName = await db.Categories
            .Where(c => c.Id == product.CategoryId)
            .Select(c => c.Name)
            .SingleAsync(cancellationToken);

        return product.ToDto(categoryName);
    }
}
