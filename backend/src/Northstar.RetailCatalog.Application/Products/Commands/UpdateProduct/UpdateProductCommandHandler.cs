using MediatR;
using Microsoft.EntityFrameworkCore;
using Northstar.RetailCatalog.Application.Abstractions;
using Northstar.RetailCatalog.Application.Common;
using Northstar.RetailCatalog.Application.Exceptions;
using Northstar.RetailCatalog.Application.Products.DTOs;
using Northstar.RetailCatalog.Domain.Common;
using Northstar.RetailCatalog.Domain.Exceptions;

namespace Northstar.RetailCatalog.Application.Products.Commands.UpdateProduct;

public sealed class UpdateProductCommandHandler(
    ICatalogDbContext db,
    TimeProvider timeProvider) : IRequestHandler<UpdateProductCommand, ProductDto>
{
    public async Task<ProductDto> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        var product = await db.Products.SingleOrDefaultAsync(p => p.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Domain.Entities.Product), request.Id);

        var input = request.Input;
        var normalizedSku = SkuNormalizer.Normalize(input.Sku);

        var categoryExists = await db.Categories.AnyAsync(c => c.Id == input.CategoryId, cancellationToken);
        if (!categoryExists)
        {
            throw new DomainRuleException($"Категория \"{input.CategoryId}\" не существует.");
        }

        var skuTaken = await db.Products.AnyAsync(
            p => p.Sku == normalizedSku && p.Id != product.Id, cancellationToken);
        if (skuTaken)
        {
            throw new SkuConflictException(normalizedSku);
        }

        product.UpdateDetails(
            normalizedSku,
            input.Name,
            input.Description,
            input.CategoryId,
            timeProvider.GetUtcNow().UtcDateTime);

        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (SkuViolation.IsUniqueViolation(ex))
        {
            throw new SkuConflictException(normalizedSku);
        }

        var categoryName = await db.Categories
            .Where(c => c.Id == product.CategoryId)
            .Select(c => c.Name)
            .SingleAsync(cancellationToken);

        return product.ToDto(categoryName);
    }
}
