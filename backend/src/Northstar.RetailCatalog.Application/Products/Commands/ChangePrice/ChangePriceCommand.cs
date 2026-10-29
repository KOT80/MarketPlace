using MediatR;
using Northstar.RetailCatalog.Application.Products.DTOs;

namespace Northstar.RetailCatalog.Application.Products.Commands.ChangePrice;

public sealed record ChangePriceCommand(Guid Id, decimal? NewPrice) : IRequest<ProductDto>;
