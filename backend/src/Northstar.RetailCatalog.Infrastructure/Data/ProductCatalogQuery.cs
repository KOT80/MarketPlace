using Microsoft.EntityFrameworkCore;
using Northstar.RetailCatalog.Application.Abstractions;
using Northstar.RetailCatalog.Application.Common;
using Northstar.RetailCatalog.Application.Products.DTOs;

namespace Northstar.RetailCatalog.Infrastructure.Data;

public sealed class ProductCatalogQuery(RetailCatalogDbContext db) : IProductCatalogQuery
{
    public async Task<PagedResult<ProductListItemDto>> ExecuteAsync(
        ProductQueryRequest request, CancellationToken cancellationToken)
    {
        var query = db.Products.AsNoTracking().AsQueryable();

        if (request.IsActive is { } isActive)
        {
            query = query.Where(p => p.IsActive == isActive);
        }

        if (request.CategoryId is { } categoryId)
        {
            query = query.Where(p => p.CategoryId == categoryId);
        }

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var pattern = $"%{EscapeLikePattern(request.Search.Trim())}%";
            query = query.Where(p =>
                EF.Functions.ILike(p.Name, pattern, "\\") ||
                EF.Functions.ILike(p.Sku, pattern, "\\"));
        }

        IOrderedQueryable<Domain.Entities.Product> ordered = ApplySort(query, request.SortBy, request.SortDirection);
        if (!request.SortBy.Equals(ProductSortFields.Sku, StringComparison.OrdinalIgnoreCase))
        {
            ordered = request.SortDirection.Equals("asc", StringComparison.OrdinalIgnoreCase)
                ? ordered.ThenBy(p => p.Id)
                : ordered.ThenByDescending(p => p.Id);
        }
        query = ordered;

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(p => new ProductListItemDto(
                p.Id,
                p.Sku,
                p.Name,
                p.CategoryId,
                p.Category.Name,
                p.Price,
                p.IsActive,
                p.CreatedAtUtc,
                p.UpdatedAtUtc))
            .ToListAsync(cancellationToken);

        return new PagedResult<ProductListItemDto>(items, request.Page, request.PageSize, totalCount);
    }

    private static string EscapeLikePattern(string value) => value
        .Replace("\\", "\\\\")
        .Replace("%", "\\%")
        .Replace("_", "\\_");

    private static IOrderedQueryable<Domain.Entities.Product> ApplySort(
        IQueryable<Domain.Entities.Product> query, string sortBy, string sortDirection)
    {
        var descending = sortDirection.Equals("desc", StringComparison.OrdinalIgnoreCase);

        return sortBy.ToLowerInvariant() switch
        {
            "name" when descending => query.OrderByDescending(p => p.Name),
            "name" => query.OrderBy(p => p.Name),
            "sku" when descending => query.OrderByDescending(p => p.Sku),
            "sku" => query.OrderBy(p => p.Sku),
            "price" when descending => query.OrderByDescending(p => p.Price),
            "price" => query.OrderBy(p => p.Price),
            _ when descending => query.OrderByDescending(p => p.CreatedAtUtc),
            _ => query.OrderBy(p => p.CreatedAtUtc),
        };
    }
}
