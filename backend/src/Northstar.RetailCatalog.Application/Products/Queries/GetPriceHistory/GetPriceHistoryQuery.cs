using MediatR;
using Northstar.RetailCatalog.Application.Products.DTOs;

namespace Northstar.RetailCatalog.Application.Products.Queries.GetPriceHistory;

public sealed record GetPriceHistoryQuery(Guid Id) : IRequest<IReadOnlyList<ProductPriceHistoryDto>>;
