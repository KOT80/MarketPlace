using MediatR;
using Microsoft.EntityFrameworkCore;
using Northstar.RetailCatalog.Application.Abstractions;
using Northstar.RetailCatalog.Application.Exceptions;
using Northstar.RetailCatalog.Application.Products.DTOs;
using Northstar.RetailCatalog.Domain.Common;
using Northstar.RetailCatalog.Domain.Entities;
using Northstar.RetailCatalog.Domain.Exceptions;

namespace Northstar.RetailCatalog.Application.Products.Commands.CreateProduct;

public sealed class CreateProductCommandHandler(
    ICatalogDbContext db,
    TimeProvider timeProvider) : IRequestHandler<CreateProductCommand, ProductDto>
{
    public async Task<ProductDto> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        var input = request.Input;
        var normalizedSku = SkuNormalizer.Normalize(input.Sku);

        var categoryExists = await db.Categories.AnyAsync(c => c.Id == input.CategoryId, cancellationToken);
        if (!categoryExists)
        {
            throw new DomainRuleException($"Категория \"{input.CategoryId}\" не существует.");
        }

        var skuTaken = await db.Products.AnyAsync(p => p.Sku == normalizedSku, cancellationToken);
        if (skuTaken)
        {
            throw new SkuConflictException(normalizedSku);
        }

        var product = Product.Create(
            normalizedSku,
            input.Name,
            input.Description,
            input.CategoryId,
            input.Price,
            timeProvider.GetUtcNow().UtcDateTime);

        db.Products.Add(product);
        await db.SaveChangesAsync(cancellationToken);

        var categoryName = await db.Categories
            .Where(c => c.Id == input.CategoryId)
            .Select(c => c.Name)
            .SingleAsync(cancellationToken);

        return product.ToDto(categoryName);
    }
}
