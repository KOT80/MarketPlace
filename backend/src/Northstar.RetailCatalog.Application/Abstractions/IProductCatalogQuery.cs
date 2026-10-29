using Northstar.RetailCatalog.Application.Common;
using Northstar.RetailCatalog.Application.Products.DTOs;

namespace Northstar.RetailCatalog.Application.Abstractions;

public interface IProductCatalogQuery
{
    Task<PagedResult<ProductListItemDto>> ExecuteAsync(ProductQueryRequest request, CancellationToken cancellationToken);
}
