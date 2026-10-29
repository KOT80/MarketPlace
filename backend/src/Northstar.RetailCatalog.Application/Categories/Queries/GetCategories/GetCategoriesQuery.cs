using MediatR;
using Northstar.RetailCatalog.Application.Categories.DTOs;

namespace Northstar.RetailCatalog.Application.Categories.Queries.GetCategories;

public sealed record GetCategoriesQuery : IRequest<IReadOnlyList<CategoryDto>>;
