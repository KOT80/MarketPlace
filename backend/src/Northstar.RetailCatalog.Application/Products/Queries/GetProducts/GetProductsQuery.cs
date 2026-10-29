using MediatR;
using Northstar.RetailCatalog.Application.Common;
using Northstar.RetailCatalog.Application.Products.DTOs;

namespace Northstar.RetailCatalog.Application.Products.Queries.GetProducts;

public sealed record GetProductsQuery(ProductQueryRequest Request) : IRequest<PagedResult<ProductListItemDto>>;
