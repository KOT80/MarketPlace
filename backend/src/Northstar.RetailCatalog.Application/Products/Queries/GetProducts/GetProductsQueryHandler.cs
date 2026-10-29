using MediatR;
using Northstar.RetailCatalog.Application.Abstractions;
using Northstar.RetailCatalog.Application.Common;
using Northstar.RetailCatalog.Application.Products.DTOs;

namespace Northstar.RetailCatalog.Application.Products.Queries.GetProducts;

public sealed class GetProductsQueryHandler(
    IProductCatalogQuery catalogQuery) : IRequestHandler<GetProductsQuery, PagedResult<ProductListItemDto>>
{
    public Task<PagedResult<ProductListItemDto>> Handle(
        GetProductsQuery request, CancellationToken cancellationToken) =>
        catalogQuery.ExecuteAsync(request.Request, cancellationToken);
}
