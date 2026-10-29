using MediatR;
using Microsoft.EntityFrameworkCore;
using Northstar.RetailCatalog.Application.Abstractions;
using Northstar.RetailCatalog.Application.Exceptions;
using Northstar.RetailCatalog.Application.Products.DTOs;
using Northstar.RetailCatalog.Domain.Entities;

namespace Northstar.RetailCatalog.Application.Products.Commands.RestoreProduct;

public sealed class RestoreProductCommandHandler(
    ICatalogDbContext db,
    TimeProvider timeProvider) : IRequestHandler<RestoreProductCommand, ProductDto>
{
    public async Task<ProductDto> Handle(RestoreProductCommand request, CancellationToken cancellationToken)
    {
        var product = await db.Products.SingleOrDefaultAsync(p => p.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Product), request.Id);

        product.Restore(timeProvider.GetUtcNow().UtcDateTime);
        await db.SaveChangesAsync(cancellationToken);

        var categoryName = await db.Categories
            .Where(c => c.Id == product.CategoryId)
            .Select(c => c.Name)
            .SingleAsync(cancellationToken);

        return product.ToDto(categoryName);
    }
}
