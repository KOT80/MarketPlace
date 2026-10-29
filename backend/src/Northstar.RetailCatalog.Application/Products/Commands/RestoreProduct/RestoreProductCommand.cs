using MediatR;
using Northstar.RetailCatalog.Application.Products.DTOs;

namespace Northstar.RetailCatalog.Application.Products.Commands.RestoreProduct;

public sealed record RestoreProductCommand(Guid Id) : IRequest<ProductDto>;
