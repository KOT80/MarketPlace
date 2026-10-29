using MediatR;
using Microsoft.EntityFrameworkCore;
using Northstar.RetailCatalog.Application.Abstractions;
using Northstar.RetailCatalog.Application.Categories.DTOs;

namespace Northstar.RetailCatalog.Application.Categories.Queries.GetCategories;

public sealed class GetCategoriesQueryHandler(
    ICatalogDbContext db) : IRequestHandler<GetCategoriesQuery, IReadOnlyList<CategoryDto>>
{
    public async Task<IReadOnlyList<CategoryDto>> Handle(
        GetCategoriesQuery request, CancellationToken cancellationToken)
    {
        var categories = await db.Categories.AsNoTracking()
            .OrderBy(c => c.Name)
            .ThenBy(c => c.Id)
            .Select(c => new CategoryDto(c.Id, c.Name, c.CreatedAtUtc))
            .ToListAsync(cancellationToken);

        return categories;
    }
}
